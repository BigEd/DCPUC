﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    [Language("DCPUC", "0.1", "C-like language for Notch's DCPU-16")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            this.LanguageFlags |= Irony.Parsing.LanguageFlags.CreateAst;

            var comment = new CommentTerminal("comment", "//", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);

            var integerLiteral = new NumberLiteral("integer", NumberOptions.IntOnly);
            integerLiteral.AddPrefix("0x", NumberOptions.Hex);
            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            identifier.AstNodeType = typeof(VariableNameNode);

            var inlineASM = new NonTerminal("inline", typeof(InlineASMNode));

            var numberLiteral = new NonTerminal("Number", typeof(NumberLiteralNode));
            var expression = new NonTerminal("Expression");
            var parenExpression = new NonTerminal("Paren Expression");
            var binaryOperation = new NonTerminal("Binary Operation", typeof(BinaryOperationNode));
            var @operator = ToTerm("+") | "-" | "*" | "/" | "%" | "&" | "|" | "^" | "==" | "!=";
            var variableDeclaration = new NonTerminal("Variable Declaration", typeof(VariableDeclarationNode));
            var dereference = new NonTerminal("Dereference", typeof(DereferenceNode));
            var statement = new NonTerminal("Statement");
            var statementList = new NonTerminal("Statement List", typeof(BlockNode));
            var assignment = new NonTerminal("Assignment", typeof(AssignmentNode));
            var ifStatement = new NonTerminal("If", typeof(IfStatementNode));
            var block = new NonTerminal("Block");
            var ifElseStatement = new NonTerminal("IfElse", typeof(IfStatementNode));
            var parameterList = new NonTerminal("Parameter List");
            var functionDeclaration = new NonTerminal("Function Declaration", typeof(FunctionDeclarationNode));
            var parameterDeclaration = new NonTerminal("Parameter Declaration");
            var parameterListDeclaration = new NonTerminal("Parameter Declaration List");
            var returnStatement = new NonTerminal("Return", typeof(ReturnStatementNode));
            //var indexOperation = new NonTerminal("Index Operation", typeof(IndexOperationNode));
            //var indexAssignment = new NonTerminal("Index Assignment", typeof(IndexAssignmentNode));
            //var inlineArgument = new NonTerminal("Inline Argument", typeof(InlineArgumentNode));
            //var inlineArgumentList = new NonTerminal("Inline Argument List");
            //var inlineInstruction = new NonTerminal("Inline Instruction", typeof(InlineInstructionNode));
            //var inlineInstructionList = new NonTerminal("Inline Instruction List");
            //var inlineBlock = new NonTerminal("Inline Block", typeof(InlineBlockNode));
            var functionCall = new NonTerminal("Function Call", typeof(FunctionCallNode));

            numberLiteral.Rule = integerLiteral;
            expression.Rule = numberLiteral | binaryOperation | parenExpression | identifier | dereference | functionCall;
            assignment.Rule = (identifier | dereference) + "=" + expression;
            binaryOperation.Rule = expression + @operator + expression;
            parenExpression.Rule = ToTerm("(") + expression + ")";
            variableDeclaration.Rule = ToTerm("var") + identifier + "=" + expression;
            dereference.Rule = ToTerm("*") + expression;
            statement.Rule = inlineASM | (variableDeclaration + ";")
                | (assignment + ";") | ifStatement | ifElseStatement | block | functionDeclaration | (functionCall + ";")
                | (returnStatement + ";");
            block.Rule = ToTerm("{") + statementList + "}";
            statementList.Rule = MakeStarRule(statementList, statement);
            inlineASM.Rule = ToTerm("asm") + "{" + new FreeTextLiteral("inline asm", "}") + "}";
            ifStatement.Rule = ToTerm("if") + "(" + expression + ")" + statement;
            ifElseStatement.Rule = ToTerm("if") + "(" + expression + ")" + statement + this.PreferShiftHere() + "else" + statement;
            parameterList.Rule = MakeStarRule(parameterList, ToTerm(","), expression);
            functionCall.Rule = identifier + "(" + parameterList + ")";
            parameterDeclaration.Rule = identifier;
            parameterListDeclaration.Rule = MakeStarRule(parameterListDeclaration, ToTerm(","), parameterDeclaration);
            functionDeclaration.Rule = ToTerm("function") + identifier + "(" + parameterListDeclaration + ")" + block;
            returnStatement.Rule = ToTerm("return") + expression;

            this.Root = statementList;

            this.RegisterBracePair("[", "]");
            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=.";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");
            this.MarkTransient(expression, parenExpression, statement, block);//, parameterList);

            this.RegisterOperators(1, Associativity.Right, "==", "!=");
            this.RegisterOperators(2, Associativity.Right, "=");
            this.RegisterOperators(3, Associativity.Left, "+", "-");
            this.RegisterOperators(4, Associativity.Left, "*", "/");
            
            this.RegisterOperators(6, Associativity.Left, "[", "]", "<", ">");
        }

    }
}
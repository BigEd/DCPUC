﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Interpreter.Ast;

namespace DCPUC
{
    class DereferenceNode : CompilableNode
    {
        public override void Init(Irony.Parsing.ParsingContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            AddChild("Expression", treeNode.ChildNodes[1]);
            
        }
       
        public override void Compile(List<string> assembly, Scope scope)
        {
            (ChildNodes[0] as CompilableNode).Compile(assembly, scope);
            assembly.Add("SET A, POP");
            assembly.Add("SET PUSH, [A]");
        }
    }

    
}

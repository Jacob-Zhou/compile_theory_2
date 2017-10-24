using compile_theory_2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compile_theory_2.Model
{
	enum SymbolKind
	{
		program,
		block,
		stmts,
		stmt,
		stmt1,
		_bool,
		_bool1,
		//_bool2,
		expr,
		expr1,
		term,
		term1,
		factor,
		IF,
		ELSE,
		WHILE,
		DO,
		BREAK,
		GT,
		GEQU,
		ADD,
		SUB,
		MULT,
		DIV,
		EQU,
		LT,
		LEQU,
		LBRA,
		RBRA,
		SEMI,
		ID,
		NUM,
		LPAR,
		RPAR,
		NULL,
		ALL
	}

	class SimpleProcess
	{
		public SimpleProcess(SymbolKind from, SymbolKind to)
		{
			this.from = from;
			this.to = to;
		}

		public SymbolKind from { get; set; }
		public SymbolKind to { get; set; }
		public int offset { get; set; }
		public int length { get; set; }
	}

	class SimpleError
	{
		public SimpleError(SimpleProcess errorProcess)
		{
			this.errorProcess = errorProcess;
		}

		public SimpleError(int line, SimpleProcess errorProcess)
		{
			this.line = line;
			this.errorProcess = errorProcess;
		}

		public int line { get; set; }
		public SimpleProcess errorProcess { get; set; }
	}

	class Parser
	{
		static private Stack<int> recoverPoint = new Stack<int>();
		static private Token token;
		static private Token OldToken;
		static private LinkedList<SymbolKind> result = new LinkedList<SymbolKind>();
		static private List<SimpleProcess> processes = new List<SimpleProcess>();
		static private List<SimpleError> errores = new List<SimpleError>();

		static private bool accapt(TokenKind tokenKind)
		{//token may null
			if (token == null)
			{
				return false;
			}
			if (tokenKind == token.kind)
			{
				OldToken = token;
				do
				{
					token = Lexer.LexNext();
					if (token == null)
					{
						token = new Token(SourceViewModel.GetEndOffset() + 1, "", TokenKind.EOF);
						break;
					}
					if(token.kind == TokenKind.ERROR)
					{
						Error err;
						err = new Error();
						if (token != null)
						{
							err.line = SourceViewModel.GetLine(token.offset);
							err.lineOffset = SourceViewModel.GetLineOffset(token.offset);
							err.length = token.value.Length;
							err.isVisable = true;
						}
						err.information = string.Format("意外的字符 {0}", token.value);
						ErrorViewModel.getInstance().addError(err);
					}
				} while (token.kind == TokenKind.ANNO || token.kind == TokenKind.ERROR);
				return true;
			}
			else
			{
				return false;
			}
		}

		static private void ShowProcess()
		{
			result.AddLast(SymbolKind.program);
			LinkedListNode<SymbolKind> replaceNode;
			LinkedListNode<SymbolKind> addNode;
			string resultStr = string.Empty;
			string production = string.Empty;
			foreach (var p in processes)
			{
				replaceNode = result.Find(p.from);
				addNode = replaceNode;
				if (replaceNode != null)
				{
					switch (p.from)
					{
						case SymbolKind.program:
							result.AddAfter(addNode, SymbolKind.block);
							result.Remove(replaceNode);
							production = "program -> block";
							break;
						case SymbolKind.block:
							addNode = result.AddAfter(addNode, SymbolKind.LBRA);
							addNode = result.AddAfter(addNode, SymbolKind.stmts);
							result.AddAfter(addNode, SymbolKind.RBRA);
							result.Remove(replaceNode);
							production = "block -> { stmts }";
							break;
						case SymbolKind.stmts:
							switch (p.to)
							{
								case SymbolKind.stmt:
									addNode = result.AddAfter(addNode, SymbolKind.stmt);
									addNode = result.AddAfter(addNode, SymbolKind.stmts);
									result.Remove(replaceNode);
									production = "stmts -> stmt stmts";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "stmts -> NULL";
									break;
							}
							break;
						case SymbolKind.stmt:
							switch (p.to)
							{
								case SymbolKind.ID:
									addNode = result.AddAfter(addNode, SymbolKind.ID);
									addNode = result.AddAfter(addNode, SymbolKind.EQU);
									addNode = result.AddAfter(addNode, SymbolKind.expr);
									result.AddAfter(addNode, SymbolKind.SEMI);
									result.Remove(replaceNode);
									production = "stmt -> ID = expr;";
									break;
								case SymbolKind.IF:
									addNode = result.AddAfter(addNode, SymbolKind.IF);
									addNode = result.AddAfter(addNode, SymbolKind.LPAR);
									addNode = result.AddAfter(addNode, SymbolKind._bool);
									addNode = result.AddAfter(addNode, SymbolKind.RPAR);
									addNode = result.AddAfter(addNode, SymbolKind.stmt);
									result.AddAfter(addNode, SymbolKind.stmt1);
									result.Remove(replaceNode);
									production = "stmt -> IF ( bool ) stmt stmt1";
									break;
								case SymbolKind.WHILE:
									addNode = result.AddAfter(addNode, SymbolKind.WHILE);
									addNode = result.AddAfter(addNode, SymbolKind.LPAR);
									addNode = result.AddAfter(addNode, SymbolKind._bool);
									addNode = result.AddAfter(addNode, SymbolKind.RPAR);
									result.AddAfter(addNode, SymbolKind.stmt);
									result.Remove(replaceNode);
									production = "stmt -> WHILE ( bool ) stmt";
									break;
								case SymbolKind.DO:
									addNode = result.AddAfter(addNode, SymbolKind.DO);
									addNode = result.AddAfter(addNode, SymbolKind.stmt);
									addNode = result.AddAfter(addNode, SymbolKind.WHILE);
									addNode = result.AddAfter(addNode, SymbolKind.LPAR);
									addNode = result.AddAfter(addNode, SymbolKind._bool);
									result.AddAfter(addNode, SymbolKind.RPAR);
									result.Remove(replaceNode);
									production = "stmt -> DO stmt WHILE ( bool )";
									break;
								case SymbolKind.BREAK:
									addNode = result.AddAfter(addNode, SymbolKind.BREAK);
									result.Remove(replaceNode);
									production = "stmt -> BREAK";
									break;
								case SymbolKind.block:
									addNode = result.AddAfter(addNode, SymbolKind.block);
									result.Remove(replaceNode);
									production = "stmt -> block";
									break;
							}
							break;
						case SymbolKind.stmt1:
							switch (p.to)
							{
								case SymbolKind.ELSE:
									addNode = result.AddAfter(addNode, SymbolKind.ELSE);
									result.AddAfter(addNode, SymbolKind.stmt);
									result.Remove(replaceNode);
									production = "stmt1 -> ELSE stmt";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "stmt1 -> NULL";
									break;
							}
							break;
						case SymbolKind._bool:
							addNode = result.AddAfter(addNode, SymbolKind.expr);
							result.AddAfter(addNode, SymbolKind._bool1);
							result.Remove(replaceNode);
							production = "bool -> expr bool1";
							break;
						case SymbolKind._bool1:
							switch (p.to)
							{
								case SymbolKind.LT:
									addNode = result.AddAfter(addNode, p.to);
									addNode = result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool1 -> < expr";
									break;
								case SymbolKind.GT:
									addNode = result.AddAfter(addNode, p.to);
									result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool1 -> > expr";
									break;
								case SymbolKind.LEQU:
									addNode = result.AddAfter(addNode, p.to);
									result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool1 -> <= expr";
									break;
								case SymbolKind.GEQU:
									addNode = result.AddAfter(addNode, p.to);
									result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool1 -> >= expr";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "bool1 -> NULL";
									break;
							}
							break;
						//case SymbolKind._bool2:
						//	switch (p.to)
						//	{
						//		case SymbolKind.expr:
						//			result.AddAfter(addNode, SymbolKind.expr);
						//			result.Remove(replaceNode);
						//			production = "bool2 -> expr";
						//			break;
						//		case SymbolKind.EQU:
						//			addNode = result.AddAfter(addNode, SymbolKind.EQU);
						//			result.AddAfter(addNode, SymbolKind.expr);
						//			result.Remove(replaceNode);
						//			production = "bool2 -> = expr";
						//			break;
						//	}
						//	break;
						case SymbolKind.expr:
							addNode = result.AddAfter(addNode, SymbolKind.term);
							result.AddAfter(addNode, SymbolKind.expr1);
							result.Remove(replaceNode);
							production = "expr -> term expr1";
							break;
						case SymbolKind.expr1:
							switch (p.to)
							{
								case SymbolKind.ADD:
									addNode = result.AddAfter(addNode, p.to);
									addNode = result.AddAfter(addNode, SymbolKind.term);
									result.AddAfter(addNode, SymbolKind.expr1);
									result.Remove(replaceNode);
									production = "expr1 -> + term expr1";
									break;
								case SymbolKind.SUB:
									addNode = result.AddAfter(addNode, p.to);
									addNode = result.AddAfter(addNode, SymbolKind.term);
									result.AddAfter(addNode, SymbolKind.expr1);
									result.Remove(replaceNode);
									production = "expr1 -> - term expr1";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "expr1 -> NULL";
									break;
							}
							break;
						case SymbolKind.term:
							addNode = result.AddAfter(addNode, SymbolKind.factor);
							result.AddAfter(addNode, SymbolKind.term1);
							result.Remove(replaceNode);
							production = "term -> factor term1";
							break;
						case SymbolKind.term1:
							switch (p.to)
							{
								case SymbolKind.MULT:
									addNode = result.AddAfter(addNode, p.to);
									addNode = result.AddAfter(addNode, SymbolKind.factor);
									result.AddAfter(addNode, SymbolKind.term1);
									result.Remove(replaceNode);
									production = "term1 -> * factor term1";
									break;
								case SymbolKind.DIV:
									addNode = result.AddAfter(addNode, p.to);
									addNode = result.AddAfter(addNode, SymbolKind.factor);
									result.AddAfter(addNode, SymbolKind.term1);
									result.Remove(replaceNode);
									production = "term1 -> / factor term1";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "term1 -> NULL";
									break;
							}
							break;
						case SymbolKind.factor:
							switch (p.to)
							{
								case SymbolKind.LPAR:
									addNode = result.AddAfter(addNode, SymbolKind.LPAR);
									addNode = result.AddAfter(addNode, SymbolKind.expr);
									result.AddAfter(addNode, SymbolKind.RPAR);
									result.Remove(replaceNode);
									production = "factor -> ( expr )";
									break;
								case SymbolKind.ID:
									result.AddAfter(addNode, SymbolKind.ID);
									result.Remove(replaceNode);
									production = "factor -> ID";
									break;
								case SymbolKind.NUM:
									result.AddAfter(addNode, SymbolKind.NUM);
									result.Remove(replaceNode);
									production = "factor -> NUM";
									break;
							}
							break;
					}
					resultStr = string.Empty;
					foreach (var s in result)
					{
						resultStr += SymbolKindToString(s);
					}
					Process process = new Process(resultStr, production);
					process.SetOffsetAddLength(p.offset, p.length);
					ProcessViewModel.Add(process);
				}
			}
		}

		static private void recoverToken()
		{
			int point = recoverPoint.Pop();
			if (point != -1)
			{
				Lexer.SetLexPosition(point);
				token = Lexer.LexNext();
			}
			errores.Clear();
		}

		static private void SaveToken()
		{
			if (token != null)
			{
				recoverPoint.Push(token.offset);
			}
			else
			{
				recoverPoint.Push(-1);
			}
		}

		static public void parse()
		{
			SourceViewModel.KeepOnlyRead();
			ProcessViewModel.Clear();
			ErrorViewModel.getInstance().clear();
			errores.Clear();
			result.Clear();
			processes.Clear();
			Lexer.Reset();
			token = Lexer.LexNext();
			OldToken = token;
			if (token == null)
			{
				SourceViewModel.UnkeepOnlyRead();
				return;
			}
			if (program())
			{
				StateViewModel.Display("成功");
				ShowProcess();
			}
			else
			{
				StateViewModel.Display("失败");
			}
			SourceViewModel.UnkeepOnlyRead();
		}

		static private string SymbolKindToString(SymbolKind s)
		{
			switch (s)
			{
				case SymbolKind._bool:
					return "bool ";
				case SymbolKind._bool1:
					return "bool1 ";
				//case SymbolKind._bool2:
				//	return "bool2 ";
				case SymbolKind.GT:
					return "> ";
				case SymbolKind.ADD:
					return "+ ";
				case SymbolKind.SUB:
					return "- ";
				case SymbolKind.MULT:
					return "* ";
				case SymbolKind.DIV:
					return "/ ";
				case SymbolKind.EQU:
					return "= ";
				case SymbolKind.LT:
					return "< ";
				case SymbolKind.LBRA:
					return "{ ";
				case SymbolKind.RBRA:
					return "} ";
				case SymbolKind.SEMI:
					return "; ";
				case SymbolKind.LPAR:
					return "( ";
				case SymbolKind.RPAR:
					return ") ";
				default:
					return s.ToString() + " ";

			}
		}

		static private string GetProduction(SimpleProcess errorProcess)
		{
			if (errorProcess != null)
			{
				switch (errorProcess.from)
				{
					case SymbolKind.program:
						return "program -> block";
					case SymbolKind.block:
						return "block -> { stmts }";
					case SymbolKind.stmts:
						switch (errorProcess.to)
						{
							case SymbolKind.stmt:
								return "stmts -> stmt stmts";
							case SymbolKind.NULL:
								return "stmts -> NULL";
							default:
								return null;
						}

					case SymbolKind.stmt:
						switch (errorProcess.to)
						{
							case SymbolKind.ID:
								return "stmt -> ID = expr;";
							case SymbolKind.IF:
								return "stmt -> IF ( bool ) stmt stmt1";

							case SymbolKind.WHILE:
								return "stmt -> WHILE ( bool ) stmt";

							case SymbolKind.DO:
								return "stmt -> DO stmt WHILE ( bool )";

							case SymbolKind.BREAK:
								return "stmt -> BREAK";

							case SymbolKind.block:
								return "stmt -> block";
							case SymbolKind.ALL:
								return "stmt -> ID = expr ;\n\t | IF ( bool ) stmt stmt1\n\t | WHILE( bool ) stmt\n\t | DO stmt WHILE ( bool )\n\t | BREAK";
							default:
								return null;
						}

					case SymbolKind.stmt1:
						switch (errorProcess.to)
						{
							case SymbolKind.ELSE:
								return "stmt1 -> ELSE stmt";

							case SymbolKind.NULL:
								return "stmt1 -> NULL";
							default:
								return null;
						}

					case SymbolKind._bool:
						return "bool -> expr bool1";

					case SymbolKind._bool1:
						switch (errorProcess.to)
						{
							case SymbolKind.LT:
								return "bool1 -> < expr";

							case SymbolKind.GT:
								return "bool1 -> > expr";

							case SymbolKind.LEQU:
								return "bool1 -> <= expr";

							case SymbolKind.GEQU:
								return "bool1 -> >= expr";

							case SymbolKind.NULL:
								return "bool1 -> NULL";
							default:
								return null;
						}

					//case SymbolKind._bool2:
					//	switch (errorProcess.to)
					//	{
					//		case SymbolKind.expr:
					//			return "bool2 -> expr";

					//		case SymbolKind.EQU:
					//			return "bool2 -> = expr";
					//		default:
					//			return null;
					//	}

					case SymbolKind.expr:
						return "expr -> term expr1";

					case SymbolKind.expr1:
						switch (errorProcess.to)
						{
							case SymbolKind.ADD:
								return "expr1 -> + term expr1";

							case SymbolKind.SUB:
								return "expr1 -> - term expr1";

							case SymbolKind.NULL:
								return "expr1 -> NULL";
							default:
								return null;
						}

					case SymbolKind.term:
						return "term -> factor term1";

					case SymbolKind.term1:
						switch (errorProcess.to)
						{
							case SymbolKind.MULT:
								return "term1 -> * factor term1";

							case SymbolKind.DIV:
								return "term1 -> / factor term1";

							case SymbolKind.NULL:
								return "term1 -> NULL";
							default:
								return null;
						}

					case SymbolKind.factor:
						switch (errorProcess.to)
						{
							case SymbolKind.LPAR:
								return "factor -> ( expr )";

							case SymbolKind.ID:
								return "factor -> ID";

							case SymbolKind.NUM:
								return "factor -> NUM";
							case SymbolKind.ALL:
								return "factor -> ( expr )\n\t | ID  NUM";
							default:
								return null;
						}
					default:
						return null;
				}
			}
			else
			{
				return null;
			}
		}

		static private void AddError(SymbolKind from, SymbolKind to, string Information)
		{
			Error err;
			err = new Error();
			if (token != null)
			{
				err.line = SourceViewModel.GetLine(token.offset);
				err.lineOffset = SourceViewModel.GetLineOffset(token.offset);
				err.length = token.value.Length;
				err.isVisable = true;
			}
			err.value = GetProduction(new SimpleProcess(from, to));
			err.information = Information;
			ErrorViewModel.getInstance().addError(err);
		}

		static private bool program()
		{

			processes.Add(new SimpleProcess(SymbolKind.program, SymbolKind.block));
			int pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;

			if (!block())
			{
				return false;
			}

			if (!accapt(TokenKind.EOF))
			{
				AddError(SymbolKind.program, SymbolKind.block, "多余的符号串");
				return false;
			}

			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
			return true;
		}

		static private bool block()
		{
			processes.Add(new SimpleProcess(SymbolKind.block, SymbolKind.LBRA));
			int pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;

			if (!accapt(TokenKind.LBRA))
			{
				AddError(SymbolKind.block, SymbolKind.LBRA, "缺少预期符号 {");
				return false;
			}

			if (!stmts())
			{
				return false;
			}

			if (!accapt(TokenKind.RBRA))
			{
				AddError(SymbolKind.block, SymbolKind.LBRA, "缺少预期符号 }");
				return false;
			}

			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
			return true;
		}

		static private bool stmts()
		{
			if (token == null)
			{
				AddError(SymbolKind.NULL, SymbolKind.NULL, "异常的结尾");
				return false;
			}
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.ID:
				case TokenKind.IF:
				case TokenKind.WHILE:
				case TokenKind.DO:
				case TokenKind.BREAK:
				case TokenKind.LBRA:
					processes.Add(new SimpleProcess(SymbolKind.stmts, SymbolKind.stmt));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;

					if (!stmt())
					{
						return false;
					}

					if (!stmts())
					{
						return false;
					}
					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				default:
					processes.Add(new SimpleProcess(SymbolKind.stmts, SymbolKind.NULL));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					processes[pIndex].length = 0;
					return true;
			}
		}

		static private bool stmt()
		{
			if (token == null)
			{
				AddError(SymbolKind.NULL, SymbolKind.NULL, "异常的结尾");
				return false;
			}
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.ID:
					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.ID));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;

					if (!accapt(TokenKind.ID))
					{
						AddError(SymbolKind.stmt, SymbolKind.ID, "内部错误");
						return false;
					}

					if (!accapt(TokenKind.EQU))
					{
						AddError(SymbolKind.stmt, SymbolKind.ID, "缺少预期符号 =");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					if (!accapt(TokenKind.SEMI))
					{
						AddError(SymbolKind.stmt, SymbolKind.ID, "缺少预期符号 ;");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.IF:
					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.IF));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;

					if (!accapt(TokenKind.IF))
					{
						AddError(SymbolKind.stmt, SymbolKind.IF, "内部错误");
						return false;
					}

					if (!accapt(TokenKind.LPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.IF, "缺少预期符号 (");
						return false;
					}

					if (!_bool())
					{
						return false;
					}

					if (!accapt(TokenKind.RPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.IF, "缺少预期符号 )");
						return false;
					}

					if (!stmt())
					{
						return false;
					}

					if (!stmt1())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.WHILE:
					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.WHILE));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;

					if (!accapt(TokenKind.WHILE))
					{
						AddError(SymbolKind.stmt, SymbolKind.WHILE, "内部错误");
						return false;
					}

					if (!accapt(TokenKind.LPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.WHILE, "缺少预期符号 (");
						return false;
					}

					if (!_bool())
					{
						return false;
					}

					if (!accapt(TokenKind.RPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.WHILE, "缺少预期符号 )");
						return false;
					}

					if (!stmt())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.DO:
					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.DO));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.DO))
					{
						AddError(SymbolKind.stmt, SymbolKind.DO, "内部错误");
						return false;
					}

					if (!stmt())
					{
						return false;
					}

					if (!accapt(TokenKind.WHILE))
					{
						AddError(SymbolKind.stmt, SymbolKind.DO, "缺少关键字 while");
						return false;
					}

					if (!accapt(TokenKind.LPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.DO, "缺少预期符号 (");
						return false;
					}

					if (!_bool())
					{
						return false;
					}

					if (!accapt(TokenKind.RPAR))
					{
						AddError(SymbolKind.stmt, SymbolKind.DO, "缺少预期符号 )");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.BREAK:
					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.BREAK));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;

					if (!accapt(TokenKind.BREAK))
					{
						AddError(SymbolKind.stmt, SymbolKind.BREAK, "内部错误");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.LBRA:

					processes.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.block));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!block())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				default:
					AddError(SymbolKind.stmt, SymbolKind.ALL, "找不到合法的后续符号");
					return false;
			}
		}

		static private bool stmt1()
		{
			int pIndex;
			if (token.kind == TokenKind.ELSE)
			{
				processes.Add(new SimpleProcess(SymbolKind.stmt1, SymbolKind.ELSE));
				pIndex = processes.Count - 1;
				processes[pIndex].offset = token.offset;
				if (!accapt(TokenKind.ELSE))
				{
					AddError(SymbolKind.stmt1, SymbolKind.ELSE, "内部错误");
					return false;
				}

				if (!stmt())
				{
					return false;
				}

				processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
				return true;
			}

			processes.Add(new SimpleProcess(SymbolKind.stmt1, SymbolKind.NULL));
			pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;
			processes[pIndex].length = 0;
			return true;
		}

		static private bool _bool()
		{
			processes.Add(new SimpleProcess(SymbolKind._bool, SymbolKind.expr));
			int pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;
			if (!expr())
			{
				return false;
			}

			if (!_bool1())
			{
				return false;
			}

			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
			return true;
		}

		static private bool _bool1()
		{
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.LT:
					processes.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.LT));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.LT))
					{
						AddError(SymbolKind._bool1, SymbolKind.LT, "内部错误");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.GT:
					processes.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.GT));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.GT))
					{
						AddError(SymbolKind._bool1, SymbolKind.GT, "内部错误");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.LEQU:
					processes.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.LEQU));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.LEQU))
					{
						AddError(SymbolKind._bool1, SymbolKind.LEQU, "内部错误");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				case TokenKind.GEQU:
					processes.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.GEQU));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.GEQU))
					{
						AddError(SymbolKind._bool1, SymbolKind.GEQU, "内部错误");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;

				default:

					processes.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.NULL));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					processes[pIndex].length = 0;
					return true;
			}
		}

		//static private bool _bool2()
		//{
		//	int pIndex;
		//	switch (token.kind)
		//	{
		//		case TokenKind.LPAR:
		//		case TokenKind.ID:
		//		case TokenKind.NUM:
		//			processes.Add(new SimpleProcess(SymbolKind._bool2, SymbolKind.expr));
		//			pIndex = processes.Count - 1;
		//			processes[pIndex].offset = token.offset;
		//			if (!expr())
		//			{
		//				return false;
		//			}

		//			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
		//			return true;

		//		case TokenKind.EQU:
		//			processes.Add(new SimpleProcess(SymbolKind._bool2, SymbolKind.EQU));
		//			pIndex = processes.Count - 1;
		//			processes[pIndex].offset = token.offset;

		//			if (!accapt(TokenKind.EQU))
		//			{
		//				AddError(SymbolKind._bool2, SymbolKind.EQU, "内部错误");
		//				return false;
		//			}

		//			if (!expr())
		//			{
		//				return false;
		//			}

		//			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
		//			return true;

		//		default:
		//			AddError(SymbolKind._bool2, SymbolKind.ALL, "找不到合法的后续符号");
		//			return false;
		//	}
		//}

		static private bool expr()
		{
			processes.Add(new SimpleProcess(SymbolKind.expr, SymbolKind.term));
			int pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;
			if (!term())
			{
				return false;
			}

			if (!expr1())
			{
				return false;
			}

			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
			return true;
		}

		static private bool expr1()
		{
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.ADD:
					processes.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.ADD));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.ADD))
					{
						AddError(SymbolKind.expr1, SymbolKind.ADD, "内部错误");
						return false;
					}

					if (!term())
					{
						return false;
					}

					if (!expr1())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				case TokenKind.SUB:
					processes.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.SUB));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.SUB))
					{
						AddError(SymbolKind.expr1, SymbolKind.SUB, "内部错误");
						return false;
					}

					if (!term())
					{
						return false;
					}

					if (!expr1())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				default:
					processes.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.NULL));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					processes[pIndex].length = 0;
					return true;
			}
		}

		static private bool term()
		{
			processes.Add(new SimpleProcess(SymbolKind.term, SymbolKind.factor));
			int pIndex = processes.Count - 1;
			processes[pIndex].offset = token.offset;
			if (!factor())
			{
				return false;
			}

			if (!term1())
			{
				return false;
			}

			processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
			return true;
		}

		static private bool term1()
		{
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.MULT:
					processes.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.MULT));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.MULT))
					{
						AddError(SymbolKind.term1, SymbolKind.MULT, "内部错误");
						return false;
					}

					if (!factor())
					{
						return false;
					}

					if (!term1())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				case TokenKind.DIV:
					processes.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.DIV));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.DIV))
					{
						AddError(SymbolKind.term1, SymbolKind.DIV, "内部错误");
						return false;
					}

					if (!factor())
					{
						return false;
					}

					if (!term1())
					{
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				default:
					processes.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.NULL));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					processes[pIndex].length = 0;
					return true;
			}
		}

		static private bool factor()
		{
			int pIndex;
			switch (token.kind)
			{
				case TokenKind.LPAR:
					processes.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.LPAR));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.LPAR))
					{
						AddError(SymbolKind.factor, SymbolKind.LPAR, "内部错误");
						return false;
					}

					if (!expr())
					{
						return false;
					}

					if (!accapt(TokenKind.RPAR))
					{
						AddError(SymbolKind.factor, SymbolKind.LPAR, "缺少预期符号 )");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				case TokenKind.ID:
					processes.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.ID));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.ID))
					{
						AddError(SymbolKind.factor, SymbolKind.ID, "内部错误");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				case TokenKind.NUM:
					processes.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.NUM));
					pIndex = processes.Count - 1;
					processes[pIndex].offset = token.offset;
					if (!accapt(TokenKind.NUM))
					{
						AddError(SymbolKind.factor, SymbolKind.NUM, "内部错误");
						return false;
					}

					processes[pIndex].length = OldToken.offset - processes[pIndex].offset + OldToken.value.Length;
					return true;
				default:
					AddError(SymbolKind.factor, SymbolKind.ALL, "找不到合法的后续符号");
					return false;
			}
		}
	}
}

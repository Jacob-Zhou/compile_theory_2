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
		_bool2,
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
		ADD,
		SUB,
		MULT,
		DIV,
		EQU,
		LT,
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
		static private LinkedList<SymbolKind> result = new LinkedList<SymbolKind>();
		static private List<SimpleProcess> processes = new List<SimpleProcess>();
		static private List<SimpleError> errores = new List<SimpleError>();

		static private bool accapt(TokenKind tokenKind)
		{
			if (token == null)
			{
				return false;
			}
			if (tokenKind == token.kind)
			{
				do
				{
					token = Lexer.LexNext();
					if (token == null)
					{
						break;
					}
				} while (token.kind == TokenKind.ANNO);
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
									addNode = result.AddAfter(addNode, SymbolKind._bool2);
									result.Remove(replaceNode);
									production = "bool1 -> < bool2";
									break;
								case SymbolKind.GT:
									addNode = result.AddAfter(addNode, p.to);
									result.AddAfter(addNode, SymbolKind._bool2);
									result.Remove(replaceNode);
									production = "bool1 -> > bool2";
									break;
								case SymbolKind.NULL:
									result.Remove(replaceNode);
									production = "bool1 -> NULL";
									break;
							}
							break;
						case SymbolKind._bool2:
							switch (p.to)
							{
								case SymbolKind.expr:
									result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool2 -> expr";
									break;
								case SymbolKind.EQU:
									addNode = result.AddAfter(addNode, SymbolKind.EQU);
									result.AddAfter(addNode, SymbolKind.expr);
									result.Remove(replaceNode);
									production = "bool2 -> = expr";
									break;
							}
							break;
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
					ProcessViewModel.Add(new Process(resultStr, production));
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
			if(token != null)
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
				ErrorHandle();
			}
			SourceViewModel.UnkeepOnlyRead();
		}

		static private string SymbolKindToString(SymbolKind s)
		{
			switch (s)
			{
				case SymbolKind._bool:
					return  "bool ";
				case SymbolKind._bool1:
					return  "bool1 ";
				case SymbolKind._bool2:
					return  "bool2 ";
				case SymbolKind.GT:
					return  "> ";
				case SymbolKind.ADD:
					return  "+ ";
				case SymbolKind.SUB:
					return  "- ";
				case SymbolKind.MULT:
					return  "* ";
				case SymbolKind.DIV:
					return  "/ ";
				case SymbolKind.EQU:
					return  "= ";
				case SymbolKind.LT:
					return  "< ";
				case SymbolKind.LBRA:
					return  "{ ";
				case SymbolKind.RBRA:
					return  "} ";
				case SymbolKind.SEMI:
					return  "; ";
				case SymbolKind.LPAR:
					return  "( ";
				case SymbolKind.RPAR:
					return  ") ";
				default:
					return  s.ToString() + " ";

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
								return "bool1 -> < bool2";

							case SymbolKind.GT:
								return "bool1 -> > bool2";

							case SymbolKind.NULL:
								return "bool1 -> NULL";
							default:
								return null;
						}

					case SymbolKind._bool2:
						switch (errorProcess.to)
						{
							case SymbolKind.expr:
								return "bool2 -> expr";

							case SymbolKind.EQU:
								return "bool2 -> = expr";
							default:
								return null;
						}

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

		static private void AddError(SymbolKind from, SymbolKind to)
		{
			if(token!= null)
			{
				errores.Add(new SimpleError(
					SourceViewModel.GetLine(token.offset),
					new SimpleProcess(from, to)));
			}
			else
			{
				errores.Add(new SimpleError(
					SourceViewModel.GetLineCount(),
					new SimpleProcess(from, to)));
			}
		}

		static private void ErrorHandle()
		{
			Error err;
			foreach (var e in errores)
			{
				err = new Error();
				err.line = e.line;
				err.value = GetProduction(e.errorProcess);
				err.infomation = "出现某些语法错误";
				ErrorViewModel.getInstance().addError(err);
			}
		}

		static private bool program()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if(block())
			{
				temp.Add(new SimpleProcess(SymbolKind.program, SymbolKind.block));
				temp.AddRange(processes);
				processes = temp;
				return true;
			}
			return false;
		}

		static private bool block()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.LBRA))
			{
				SaveToken();
				if (!stmts())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.RBRA))
				{

					AddError(SymbolKind.block, SymbolKind.LBRA);
					return false;
				}

				temp.Add(new SimpleProcess(SymbolKind.block, SymbolKind.LBRA));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			
			AddError(SymbolKind.block, SymbolKind.LBRA);
			return false;
		}

		static private bool stmts()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			SaveToken();
			if (stmt())
			{
				recoverPoint.Pop();
				SaveToken();
				if (!stmts())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.stmts, SymbolKind.stmt));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			recoverToken();

			temp.Add(new SimpleProcess(SymbolKind.stmts, SymbolKind.NULL));
			processes = temp;
			//errores.Clear();
			return true;
		}

		static private bool stmt()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.ID))
			{
				if (!accapt(TokenKind.EQU))
				{
					AddError(SymbolKind.stmt, SymbolKind.ID);
					return false;
				}

				SaveToken();
				if (!expr())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.SEMI))
				{
					AddError(SymbolKind.stmt, SymbolKind.ID);
					return false;
				}

				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.ID));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.IF))
			{
				if (!accapt(TokenKind.LPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.IF);
					return false;
				}

				SaveToken();
				if (!_bool())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.RPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.IF);
					return false;
				}

				SaveToken();
				if (!stmt())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				SaveToken();
				if (!stmt1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.IF));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.WHILE))
			{
				if (!accapt(TokenKind.LPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.WHILE);
					return false;
				}

				SaveToken();
				if (!_bool())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.RPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.WHILE);
					return false;
				}

				SaveToken();
				if (!stmt())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.WHILE));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.DO))
			{
				SaveToken();
				if (!stmt())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.WHILE))
				{
					AddError(SymbolKind.stmt, SymbolKind.DO);
					return false;
				}

				if (!accapt(TokenKind.LPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.DO);
					return false;
				}

				SaveToken();
				if (!_bool())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.RPAR))
				{
					AddError(SymbolKind.stmt, SymbolKind.DO);
					return false;
				}
				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.DO));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.BREAK))
			{
				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.BREAK));
				processes = temp;
				//errores.Clear();
				return true;
			}

			SaveToken();
			processes.Clear();
			if (block())
			{
				recoverPoint.Pop();
				temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.block));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			recoverToken();

			AddError(SymbolKind.stmt, SymbolKind.ALL);
			return false;
		}

		static private bool stmt1()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.ELSE))
			{
				SaveToken();
				if (!stmt())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.stmt1, SymbolKind.ELSE));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			temp.Add(new SimpleProcess(SymbolKind.stmt1, SymbolKind.NULL));
			processes = temp;
			//errores.Clear();
			return true;
		}

		static private bool _bool()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			SaveToken();
			if (expr())
			{
				recoverPoint.Pop();
				SaveToken();
				if (!_bool1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind._bool, SymbolKind.expr));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			recoverToken();
			return false;
		}

		static private bool _bool1()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.LT))
			{
				SaveToken();
				if (!_bool2())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.LT));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.GT))
			{
				SaveToken();
				if (!_bool2())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.GT));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			temp.Add(new SimpleProcess(SymbolKind._bool1, SymbolKind.NULL));
			processes = temp;
			//errores.Clear();
			return true;
		}

		static private bool _bool2()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();

			
			if (accapt(TokenKind.EQU))
			{
				SaveToken();
				if (!expr())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind._bool2, SymbolKind.EQU));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}


			SaveToken();
			processes.Clear();
			if (expr())
			{
				recoverPoint.Pop();
				temp.Add(new SimpleProcess(SymbolKind._bool2, SymbolKind.expr));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			recoverToken();
			temp.Add(new SimpleProcess(SymbolKind.stmt, SymbolKind.DO));
			temp.AddRange(processes);
			processes = temp;
			return false;
		}

		static private bool expr()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();

			SaveToken();
			if (term())
			{
				recoverPoint.Pop();
				SaveToken();
				if (!expr1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.expr, SymbolKind.term));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			recoverToken();
			return false;
		}

		static private bool expr1()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.ADD))
			{
				SaveToken();
				if (!term())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				SaveToken();
				if (!expr1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.ADD));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.SUB))
			{
				SaveToken();
				if (!term())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				SaveToken();
				if (!expr1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.SUB));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			temp.Add(new SimpleProcess(SymbolKind.expr1, SymbolKind.NULL));
			processes = temp;
			//errores.Clear();
			return true;
		}

		static private bool term()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();

			SaveToken();
			if (factor())
			{
				recoverPoint.Pop();
				SaveToken();
				if (!term1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.term, SymbolKind.factor));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			recoverToken();

			return false;
		}

		static private bool term1()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.MULT))
			{
				SaveToken();
				if (!factor())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				SaveToken();
				if (!term1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.MULT));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.DIV))
			{
				SaveToken();
				if (!factor())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				SaveToken();
				if (!term1())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				temp.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.DIV));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}
			temp.Add(new SimpleProcess(SymbolKind.term1, SymbolKind.NULL));
			processes = temp;
			//errores.Clear();
			return true;
		}

		static private bool factor()
		{
			List<SimpleProcess> temp = processes;
			processes = new List<SimpleProcess>();
			if (accapt(TokenKind.LPAR))
			{
				SaveToken();
				if (!expr())
				{
					recoverToken();
					return false;
				}
				recoverPoint.Pop();

				if (!accapt(TokenKind.RPAR))
				{
					AddError(SymbolKind.factor, SymbolKind.LPAR);
					return false;
				}

				temp.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.LPAR));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.ID))
			{
				temp.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.ID));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			processes.Clear();
			if (accapt(TokenKind.NUM))
			{
				temp.Add(new SimpleProcess(SymbolKind.factor, SymbolKind.NUM));
				temp.AddRange(processes);
				processes = temp;
				//errores.Clear();
				return true;
			}

			AddError(SymbolKind.factor, SymbolKind.ALL);
			return false;
		}
	}
}

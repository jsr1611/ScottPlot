﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlotTests
{
    public static class Report
    {
        class CodeSnippit
        {
            readonly public string code;
            readonly public string pic;

            public CodeSnippit(string code, string pic)
            {
                this.code = code;
                this.pic = pic;
            }

            public string codeHTML
            {
                get
                {
                    string html = code;
                    html = html.Replace("\n", "<br>");
                    html = html.Replace(" ", "&nbsp;");
                    return html;
                }
            }
        }

        static List<CodeSnippit> GetCodeSnippits()
        {
            // read this source code file and return a list of code snippits associated with PNG files
            var codeSnippits = new List<CodeSnippit>();

            // read the contents of this source code file
            string sourceCodeFilePath = System.IO.Path.GetFullPath("../../ScottPlotTester.cs");
            string sourceCode = System.IO.File.ReadAllText(sourceCodeFilePath);

            // global search/replace can be done here
            sourceCode = sourceCode.Replace("demoSizeX", "600");
            sourceCode = sourceCode.Replace("demoSizeY", "400");
            sourceCode = sourceCode.Replace("tests/images/", "");

            // break the source code into functions
            string[] splitWord = new string[] { "void " };
            string[] chunks = sourceCode.Split(splitWord, StringSplitOptions.RemoveEmptyEntries);
            foreach (string chunk in chunks)
            {
                // keep only functions which save images
                if (!chunk.Contains("plt.figure.Save("))
                    continue;
                // ignore this function
                if (chunk.Contains("ignore this function"))
                    continue;

                string[] lines = chunk.Trim().Split('\n');
                string picFileName = "???";
                string code = "";
                for (int i = 2; i < lines.Length - 1; i++)
                {
                    string line = lines[i];
                    if (!line.StartsWith("            "))
                        break;
                    line = line.Replace("            ", "");
                    code += line + "\n";
                    if (line.StartsWith("plt.figure.Save"))
                    {
                        line = line.Replace('\'', '"');
                        string[] fileSaveLine = line.Split('"');
                        picFileName = fileSaveLine[1];
                        picFileName = System.IO.Path.GetFileName(picFileName);
                    }
                }
                codeSnippits.Add(new CodeSnippit(code.Trim(), picFileName));
            }
            return codeSnippits;
        }

        public static void MakeHTML()
        {
            var codeSnippits = GetCodeSnippits();

            string html = "";
            foreach (var snippit in codeSnippits)
            {
                html += $"<div class='image'><a href='images/{snippit.pic}'><img src='images/{snippit.pic}' width='500'></a></div>";
                html += $"<div class='codeblock'><code class='prettyprint cs'>{snippit.codeHTML}</code></div>";
            }
            string head = "";
            head += "<script src='https://cdn.rawgit.com/google/code-prettify/master/loader/run_prettify.js'></script>";
            string styles = "";
            styles += ".image {margin-top: 50px;}";
            styles += ".codeblock {display: table; width: auto; background-color: #F6F6F6; border: 2px solid #DDD; padding: 10px;}";
            html = $"<html><head>{head}</head><style>{styles}</style><body>{html}</body></html>";
            System.IO.File.WriteAllText("tests/readme.html", html);
        }

        public static void MakeMarkdown()
        {
            var codeSnippits = GetCodeSnippits();

            string md = "# ScottPlot Cookbook\n\n";
            md += "This page was automatically generated by [ScottPlotTests](https://github.com/swharden/ScottPlot/tree/master/test/ScottPlotTests) ";
            md += "(a Console Application). In these examples, `xs` and `ys` are large double arrays.\n\n";
            foreach (var snippit in codeSnippits)
            {
                md += $"## {snippit.pic}\n";
                md += $"![](images/{snippit.pic})\n";
                md += $"```\n{snippit.code}\n```\n\n";
            }
            System.IO.File.WriteAllText("tests/readme.md", md);
        }
    }
}
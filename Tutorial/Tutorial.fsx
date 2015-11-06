// ----------------------------------------------------------------------------
// Reference F# Compiler Service and F# Formatting
// ----------------------------------------------------------------------------

#r "../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#load "../paket-files/matthid/Yaaf.FSharp.Scripting/src/source/Yaaf.FSharp.Scripting/YaafFSharpScripting.fs"
#load "../packages/FSharp.Formatting/FSharp.Formatting.fsx"
open System.IO
open System.Text
open FSharp.Literate
open FSharp.Markdown
open FSharp.CodeFormat
open Yaaf.FSharp.Scripting
open Microsoft.FSharp.Compiler.Interactive.Shell

// ----------------------------------------------------------------------------
// PART #1: Reading paragraphs from a literate document
// ----------------------------------------------------------------------------

// Load "Sample.fsx" in the current directory
let sample = __SOURCE_DIRECTORY__ + "/Sample.fsx"
let doc = Literate.ParseScriptFile(sample)

// Print the type names of the individual paragraphs
// (This prints names of cases in "MarkdownParagraph"
// so we can see things like "Heading", "ListBlock"
// and "EmbedParagraph" for inline code.)
for par in doc.Paragraphs do
  printfn "%O" par

// Get the first paragraph
let first = doc.Paragraphs |> Seq.head

// This should be a heading - heading contains size and
// the text - but the text can have inline formatting using
// "MarkdownSpan" (like bold, etc.). Try typing the following
// to see the different types of content available:
//
//    MarkdownParagraph.
//    MarkdownSpan.
match first with
| MarkdownParagraph.Heading(n, [ MarkdownSpan.Literal text ]) -> 
    printfn "%s" text
| _ -> printfn "Not a heading!"

// ----------------------------------------------------------------------------
// TASK #1: Write a recursive function that will take
// the paragraphs from "Sample.fsx" and split them into
// individual performance tests.
//
// To do this, you need to look for Heading - each 
// performance test starts with a heading that contains
// its name.
// ----------------------------------------------------------------------------

// Now, we'll need to get the F# code. The code snippets
// are stored using a special type "LiterateParagraph" which
// is embedded in the document using "EmbedParagraphs" node.
// (The Markdown document does not know about these.)
// The following helper extract the "LiterateParagraph" node.
let extractLiterateCodeBlock embed =
  match embed with
  | MarkdownParagraph.EmbedParagraphs
      (:? LiterateParagraph as lit) -> lit
  | _ -> failwith "Not a literate code block!"

// To see how this works, let's get the 3rd paragraph
let embed = doc.Paragraphs |> Seq.nth 2
let code = extractLiterateCodeBlock embed

// You can now pattern match on "LiterateParagraph" (here,
// we only get "FormattedCode"). The following snippet 
// iterates over the lines and the tokens and prints the
// source code (you can also see tool tips, token kinds etc.)
match code with
| LiterateParagraph.FormattedCode(lines) -> 
    for (Line tokens) in lines do 
      for tok in tokens do
        match tok with
        | TokenSpan.Token(kind, text, tooltip) ->
            printf "%s" text
        | _ -> ()
      printfn ""
| _ -> 
    printfn "Some other code block"

// ----------------------------------------------------------------------------
// TASK #2: Now you have everything you need to extract 
// the information about the performance tests! Using
// the groups that you get from Task #1 and the snippets
// above, you should be able to turn the groups into 
// values of the following F# record:

type PerfTest = 
  { Name : string 
    Repeat : int
    Run : string 
    Body : string }

// ----------------------------------------------------------------------------

// Next, we need to evaluate the performance using 
// embedded F# interactive. Here is the 'Evaluator'
// helper from the previous demos:

let internal fsiSession = ScriptHost.Create(FsiOptions.Default, preventStdOut=true)

// To see how you can evaluate the performance, 
// let's create a sample silly "PerfTest" instance:
let test = 
  { Name = "Silly test!"; Repeat = 10; Run = "test()" 
    Body = """
      let test() = 
         System.Threading.Thread.Sleep(100) """ }

// The evaluator provides two methods - "EvalInteraction"
// runs a statement (such as function definition) and does
// not return any result - we can use this to define the 
// function we want to test before evaluating it:
fsiSession.EvalInteraction(test.Body)

// Once we have this, we can run the expression 
// e.g. "test()" and measure how long it takes
let sw = System.Diagnostics.Stopwatch.StartNew()
fsiSession.EvalExpression<unit>(test.Run)
printfn "Evaluated in: %d ms" sw.ElapsedMilliseconds

// ----------------------------------------------------------------------------
// TASK #3: Use the "Evaluator" to evaluate the performance of 
// the tests from "Sample.fsx"! Your code should print summary
// report that looks something like this:
//
//  * Calculating factorial (1000 times) - 113 ms
//  * Working with large arrays (10 times) - 642 ms
//  * String concatenation (200 times) - 289 ms
//
// ----------------------------------------------------------------------------

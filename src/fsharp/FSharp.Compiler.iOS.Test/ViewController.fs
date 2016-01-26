namespace FSharp.Compiler.iOS.Test

open System
open System.Drawing

open Foundation
open UIKit


[<Register ("ViewController")>]
type ViewController (handle:IntPtr) =
    inherit UIViewController (handle)

    override x.DidReceiveMemoryWarning () =
        // Releases the view if it doesn't have a superview.
        base.DidReceiveMemoryWarning ()
        // Release any cached data, images, etc that aren't in use.

    override x.ViewDidLoad () =
        base.ViewDidLoad ()

        System.Threading.ThreadPool.QueueUserWorkItem (fun _ ->
            try
                let libDir = IO.Path.GetDirectoryName (typeof<ViewController>).Assembly.Location
                let compiler = new Microsoft.FSharp.Compiler.Driver.InProcCompiler ()
                let exitCode, output = compiler.Compile [| "fsc"; "foo.fs" |]
                for e in output.Errors do
                    match e with
                    | Microsoft.FSharp.Compiler.CompileOps.ErrorOrWarning.Long (_, info) -> printfn "OUTPUT ERROR %A" info.Message
                    | _ -> printfn "OUTPUT ERROR %O" e
                for e in output.Warnings do
                    printfn "OUTPUT WARNING %A" e
            with ex ->
                Console.WriteLine (ex)
            ) |> ignore

        // Perform any additional setup after loading the view, typically from a nib.

    override x.ShouldAutorotateToInterfaceOrientation (toInterfaceOrientation) =
        // Return true for supported orientations
        if UIDevice.CurrentDevice.UserInterfaceIdiom = UIUserInterfaceIdiom.Phone then
           toInterfaceOrientation <> UIInterfaceOrientation.PortraitUpsideDown
        else
           true


namespace Microsoft.Build.Framework

type BuildEventArgs () =
    inherit System.EventArgs ()
    member this.Message = ""

type LazyFormattedBuildEventArgs () =
    inherit BuildEventArgs ()

type CustomBuildEventArgs () =
    inherit LazyFormattedBuildEventArgs ()

type BuildErrorEventArgs () =
    inherit LazyFormattedBuildEventArgs ()
    member this.Code = ""

type BuildMessageEventArgs () =
    inherit LazyFormattedBuildEventArgs ()
    member this.Code = ""

type BuildWarningEventArgs () =
    inherit LazyFormattedBuildEventArgs ()
    member this.Code = ""





namespace Microsoft.Build.Utilities

open System.Collections.Generic

type ITaskItem =
    abstract ItemSpec : string with get,set
    abstract SetMetadata : string*string -> unit
    abstract GetMetadata : string -> string

type TaskItem (name) =
    let metadata = new Dictionary<string, string> ()
    member val ItemSpec = name with get,set
    member this.SetMetadata (k, v) = metadata.[k] <- v
    member this.GetMetadata (k) = metadata.[k]
    interface ITaskItem with
        member this.SetMetadata (k, v) = this.SetMetadata (k, v)
        member this.GetMetadata (k) = this.GetMetadata (k)
        member this.ItemSpec with get() = this.ItemSpec and set v = this.ItemSpec <- v


type ITask =
    abstract Execute : unit -> bool

[<AbstractClass>]
type Task () =
    abstract Execute : unit -> bool
    interface ITask with
        member this.Execute () = this.Execute ()



namespace Microsoft.Build.BuildEngine

open System.Collections
open Microsoft.Build.Framework

type IBuildEngine =
    abstract BuildProjectFile : string * string[] * IDictionary * IDictionary -> bool
    abstract LogCustomEvent : CustomBuildEventArgs -> unit
    abstract LogErrorEvent : BuildErrorEventArgs -> unit
    abstract LogMessageEvent : BuildMessageEventArgs -> unit
    abstract LogWarningEvent : BuildWarningEventArgs -> unit
    abstract ColumnNumberOfTaskNode : int
    abstract LineNumberOfTaskNode : int
    abstract ContinueOnError : bool
    abstract ProjectFileOfTaskNode : string



namespace Microsoft.Build.Tasks

open Microsoft.Build.Utilities
open Microsoft.Build.BuildEngine

[<AbstractClass>]
type TaskExtension () =
    inherit Task ()

module Defaults =
    let engine = 
            { new IBuildEngine with 
                    member __.BuildProjectFile(projectFileName, targetNames, globalProperties, targetOutputs) = true
                    member __.LogCustomEvent(e) = ()
                    member __.LogErrorEvent(e) = ()
                    member __.LogMessageEvent(e) = ()
                    member __.LogWarningEvent(e) = ()
                    member __.ColumnNumberOfTaskNode = 1
                    member __.LineNumberOfTaskNode = 1
                    member __.ContinueOnError = true
                    member __.ProjectFileOfTaskNode = "" }

type ResolveAssemblyReference () =
    inherit TaskExtension ()

    let resolveAsm (asm : ITaskItem) : ITaskItem option =
        try
            let path = asm.ItemSpec
            let fullName = asm.ItemSpec
            let version = "3.14159"
            let t = TaskItem (path)
            t.SetMetadata ("ResolvedFrom", "{AssemblyFolders}")
            t.SetMetadata ("FusionName", fullName)
            t.SetMetadata ("Version", version)
            t.SetMetadata ("Redist", "")
            t.SetMetadata ("Baggage", asm.GetMetadata ("Baggage"))
            Some (t :> ITaskItem)
        with ex ->
            System.Diagnostics.Debug.WriteLine (ex)
            None

    member val BuildEngine : IBuildEngine = Defaults.engine with get,set
    member val TargetFrameworkDirectories:string[]=[||] with get,set
    member val TargetProcessorArchitecture="" with get,set
    member val FindRelatedFiles=false with get,set
    member val FindDependencies=false with get,set
    member val FindSatellites=false with get,set
    member val FindSerializationAssemblies=false with get,set
    member val Assemblies : ITaskItem[] = [||] with get,set
    member val ResolvedFiles : ITaskItem[] = [||] with get,set
    member val SearchPaths : string[] = [||] with get,set
    member val AllowedAssemblyExtensions : string[] = [||] with get,set

    member val ResolvedDependencyFiles : ITaskItem[] = [||] with get,set
    member val RelatedFiles : ITaskItem[] = [||] with get,set
    member val SatelliteFiles : ITaskItem[] = [||] with get,set
    member val ScatterFiles : ITaskItem[] = [||] with get,set
    member val CopyLocalFiles : ITaskItem[] = [||] with get,set
    member val SuggestedRedirects : ITaskItem[] = [||] with get,set

    override this.Execute () =
        let r = this.Assemblies |> Array.choose resolveAsm
        this.ResolvedFiles <- r
        r.Length = this.Assemblies.Length



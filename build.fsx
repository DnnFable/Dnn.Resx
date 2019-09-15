#r "paket: groupref Build //"
// include Fake modules, see Fake modules section

open Fake.IO
open Fake.IO.FileSystemOperators 
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.Core
open Fake.Api
open Markdig
open System.Xml

//Properties
let packageName = "Dnn.Resx"
let installDir = ".install"
let buildDir = "src/bin/debug"
let packageDir = installDir </> "package"
let deployDir =  installDir </> "deploy"
let changeLogFile = "RELEASENOTES.md"
let manifestFile = (sprintf "%s.dnn" packageName )
let gitOwner = "scullman"
let gitName = packageName

[<RequireQualifiedAccess>]
module Xml =
    let appendChild name (parent:XmlElement) =
        let el = parent.OwnerDocument.CreateElement name
        parent.AppendChild (el) :?> XmlElement

    let withChild name innerText (parent:XmlElement) =
        let el = parent.OwnerDocument.CreateElement name
        el.InnerText  <- innerText
        parent.AppendChild(el) |> ignore
        parent

    let withOptionalChild name (innerText: string option) (parent:XmlElement) =
        match innerText with
        | None -> parent
        | Some text -> withChild name text parent

    let withAttribute name value (el:XmlElement) =
        el.SetAttribute (name, value)
        el

//Targets
Target.create "Clean" (fun _ ->
    Shell.cleanDir installDir
    Shell.cleanDir buildDir
    Shell.cleanDir deployDir
    Shell.cleanDir packageDir
)

// Default target
Target.create "Default" (fun _ ->
  Trace.trace <| sprintf "FAKE %s" packageName
)

Target.create "AssemblyInfo" (fun _ ->
    let changelog = changeLogFile |> Changelog.load
    AssemblyInfoFile.createCSharp  "src/Properties/AssemblyInfo.cs"
      [ AssemblyInfo.Product packageName
        AssemblyInfo.Description packageName
        AssemblyInfo.Version changelog.LatestEntry.AssemblyVersion
        AssemblyInfo.FileVersion changelog.LatestEntry.AssemblyVersion
      ]
)

let copyAssemblies folder =
    !! (buildDir </> "*.dll")
    |> Shell.copyTo folder 

let buildProjects projects =
    let silent (par:MSBuildParams) =  {par with NoWarn =Some ["MSB3245"; "MSB3243"; "FS0025"]; Verbosity=Some Minimal  }
    projects
    |> Seq.iter (fun p -> MSBuild.build silent p) 

Target.create "BuildServices" (fun _->
    !! "src/**/*.csproj"
    |> buildProjects  
)

Target.create "PackModule" (fun _ ->

    let packAssemblies () =
        copyAssemblies (packageDir </> "bin")

    let packReleaseNotes () =
        File.readAsString changeLogFile
        |> Markdown.ToHtml
        |> File.writeString true (packageDir </> "releasenotes.html")
    
    let packManifest () =
        
        let setVersion () = 
            let changelog = changeLogFile |> Changelog.load
            let manifest = packageDir </> manifestFile
            Xml.loadDoc manifest
            |> Xml.replaceXPathAttribute "/dotnetnuke/packages/package" "version" changelog.LatestEntry.AssemblyVersion
            |> Xml.saveDoc manifest

        let addAssemblies () = 
            let manifest = packageDir </> manifestFile
            Xml.loadDoc manifest
            |> fun doc ->
               let containerElement = 
                    doc.SelectSingleNode("/dotnetnuke/packages/package/components") :?> XmlElement
                    |> Xml.appendChild "component" 
                    |> Xml.withAttribute "type" "Assembly"
                    |> Xml.appendChild "assemblies"
                   
               !! (sprintf  "%s/*.dll" buildDir)
               |> Seq.iter (fun file ->
                    containerElement
                    |> Xml.appendChild "assembly"
                    |> Xml.withChild "name" (FileSystemInfo.ofPath file).Name
                    |> Xml.withChild "path" "bin"
                    |> Xml.withOptionalChild "version" (File.tryGetVersion file) 
                    |> ignore ) 
               doc
            |> Xml.saveDoc manifest

        Shell.copyFile packageDir manifestFile
        setVersion()
        addAssemblies()

    let createModulePackage () =
        let changelog = changeLogFile |> Changelog.load
        !! (packageDir </> "**/*" ) 
        |> Zip.filesAsSpecs packageDir
        |> Zip.moveToFolder "/"
        |> Zip.zipSpec  (sprintf "%s\\%s-%s-install.zip" installDir packageName (changelog.LatestEntry.AssemblyVersion) )
    
    packAssemblies()
    packReleaseNotes()
    packManifest()
    createModulePackage ()
)

Target.create "GitHubRelease" (fun _ ->
    let token =
        match Environment.environVarOrDefault "github_token" "" with
        | s when not (System.String.IsNullOrWhiteSpace s) -> s
        | _ -> failwith "please set the github_token environment variable to a github personal access token with repro access."

    let files = !! (installDir </> "*-install.zip")

    let changelog = changeLogFile |> Changelog.load
    let version = changelog.LatestEntry.AssemblyVersion
    let notes = [| defaultArg changelog.LatestEntry.Description "[TBD]" |]
        
    GitHub.createClientWithToken token
    |> GitHub.draftNewRelease gitOwner gitName version true notes
    |> GitHub.uploadFiles files
    |> GitHub.publishDraft
    |> Async.RunSynchronously)

open Fake.Core.TargetOperators

"AssemblyInfo"
    ==> "BuildServices"

"Clean" 
    ==> "BuildServices"
    ==> "PackModule"
    ==> "Default"
    ==> "GitHubRelease"

// start build
Target.runOrDefault "Default"
open it.mahamudra.imaging
open System.Drawing
open System
open System.IO
open System.Linq
   
let scriviImgsFromPdf dir_path dir_output_images = 
        let dir = new DirectoryInfo(dir_path)
        let files = dir.GetFiles("*.pdf", SearchOption.AllDirectories)
        let fullNames = files.Select(fun file -> file.FullName).ToArray();
        for pdf_file_path in fullNames do
        let imgs = PDF.extractImages pdf_file_path
        for i:Image*string*ImagesFormat*int in imgs do
            let img,filename,ext,page = i
            let id = Guid.NewGuid()
            printfn "%s" (ext.ToString())
            img.Save(sprintf @"%s%s_%s_%s%s" dir_output_images filename (page.ToString()) (id.ToString()) (ext.ToString())) |>ignore 

let scriviImgsFromPdfsInDirectory pdf_file_path dir_output_images = 
        let imgs = PDF.extractImages pdf_file_path
        for i:Image*string*ImagesFormat*int in imgs do
            let img,filename,ext,page = i
            let id = Guid.NewGuid()
            printfn "%s" (ext.ToString())
            img.Save(sprintf @"%s%s_%s_%s%s" dir_output_images filename (page.ToString()) (id.ToString()) (ext.ToString())) |>ignore 
        
        
                
[<EntryPoint>]
let main argv = 
        let context =  argv.[0]
        let pdf_file_path =  argv.[1]
        let dir_output_images = argv.[2]

        match context with
        | "dir" ->  
        |_-> scriviImgsFromPdf pdf_file_path dir_output_images

        printfn "%A" argv
        0 // return an integer exit code
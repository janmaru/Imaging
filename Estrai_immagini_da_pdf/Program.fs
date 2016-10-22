open it.mahamudra.imaging
open System.Drawing
open System
    
[<EntryPoint>]
let main argv = 
        let pdf_file_path =  @"C:\DEV\Imaging\test_imaging\1.pdf"
        let fileName = PDF.getNomeFile pdf_file_path
        let imgs = PDF.extractImages pdf_file_path
        for i:Image*ImagesFormat*int in imgs do
            let img,ext,page = i
            let id = Guid.NewGuid()
            printfn "%s" (ext.ToString())
            img.Save(sprintf @"C:\DEV\Imaging\test_imaging\%s_%s_%s%s" fileName (page.ToString()) (id.ToString()) (ext.ToString())) |>ignore 
        printfn "%A" argv
        0 // return an integer exit code

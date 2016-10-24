namespace it.mahamudra.imaging
open System

type Dangerous(comment:string) = inherit System.Attribute();
type ImagesFormat =  Bmp | Jpeg | Gif | Tiff | Png |  Unknown 
                       override this.ToString() =
                        match this with
                         | Bmp -> ".bmp"
                         | Jpeg -> ".jpeg"
                         | Gif -> ".gif"
                         | Tiff -> ".tiff"
                         | Png -> ".png"
                         | _ -> ".unk"

module IMG = 
     open System.Drawing
     open System.IO
     open System.Runtime.Serialization.Formatters.Binary
 
     let imageToByteArray(image:Image) =
        let ic = new ImageConverter()
        ic.ConvertTo(image, typeof<byte[]>):?>byte[]

     let byteArrayToImage(bytes:byte[]) =
         let ms:MemoryStream = new MemoryStream(bytes) 
         let image:Image = Image.FromStream(ms) 
         image
 
module PDF = 
    open Spire.Pdf
    open System.Drawing
    open System.Drawing.Imaging
    open System.Text
    open System.Linq
    open System.IO

    let extractExt(image:Image):ImagesFormat = 
        let bytes = IMG.imageToByteArray image
        // see http://www.mikekunz.com/image_file_header.html  
        let bmp  = Encoding.ASCII.GetBytes("BM")      // BMP
        let gif  = Encoding.ASCII.GetBytes("GIF")    // GIF
        let png  = [|137uy; 80uy; 78uy; 71uy |]    // PNG
        let tiff = [|73uy; 73uy; 42uy|]         // TIFF
        let tiff2= [|77uy; 77uy; 42uy|]        // TIFF
        let jpeg = [|255uy; 216uy; 255uy; 224uy|] // jpeg
        let jpeg2= [|255uy; 216uy; 255uy; 225uy|] // jpeg canon

        if bmp.SequenceEqual(bytes.Take(bmp.Length)) then
            ImagesFormat.Bmp
        elif gif.SequenceEqual(bytes.Take(gif.Length)) then
            ImagesFormat.Gif 
        elif png.SequenceEqual(bytes.Take(png.Length)) then
            ImagesFormat.Png 
        elif tiff.SequenceEqual(bytes.Take(tiff.Length)) then
            ImagesFormat.Tiff 
        elif  tiff2.SequenceEqual(bytes.Take(tiff2.Length)) then
            ImagesFormat.Tiff 
        elif  jpeg.SequenceEqual(bytes.Take(jpeg.Length)) then
            ImagesFormat.Jpeg 
        elif  jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)) then
            ImagesFormat.Jpeg 
        else
            ImagesFormat.Unknown 
     
    let getNomeFile(pdf_file_path:string) =
        Path.GetFileNameWithoutExtension pdf_file_path
 
    let extractImages (pdf_file_path:string) =
        let doc:PdfDocument  = new PdfDocument() 
        doc.LoadFromFile(pdf_file_path)
        let pages = doc.Pages
        let mutable count = 0
        let file_name = getNomeFile pdf_file_path
        seq { for page:PdfPageBase in pages do
                let imgs = page.ExtractImages()
                count<-count+1
                for img:Image in imgs do
                yield img, file_name, extractExt img, count}

    let createFileName dir_output_images filename page id ext = sprintf @"%s%s_%s_%s%s" dir_output_images filename (page.ToString()) (id.ToString()) (ext.ToString())

    let scriviImgsFromPdf pdf_file_path dir_output_images = 
        let imgs = extractImages pdf_file_path
        for i:Image*string*ImagesFormat*int in imgs do
            let img,filename,ext,page = i
            let id = Guid.NewGuid()
            img.Save(createFileName dir_output_images filename page id ext) |>ignore 
            
    let scriviImgsFromPdfsInDirectory dir_or_file_path dir_output_images = 
        if File.Exists(dir_or_file_path) then
           scriviImgsFromPdf dir_or_file_path dir_output_images
        elif Directory.Exists(dir_or_file_path) then  //presume is a directory
            let dir = new DirectoryInfo(dir_or_file_path)
            let files = dir.GetFiles("*.pdf", SearchOption.AllDirectories) //get only .pdf files
            let fullNames = files.Select(fun file -> file.FullName).ToArray();
            for pdf_file_path in fullNames do
                scriviImgsFromPdf pdf_file_path dir_output_images


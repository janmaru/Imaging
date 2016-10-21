namespace it.mahamudra.imaging
open System

type Dangerous(comment:string) = inherit System.Attribute();
type ImagesFormat =  Bmp | Jpeg | Gif | Tiff | Png |  Unknown

module IMG = 
     open System.Drawing
     open System.IO
     let imageToByteArray(image:Image) =
         let ms:MemoryStream = new MemoryStream() 
         image.Save(ms, image.RawFormat)
         ms.ToArray() 

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

     let extractImages (pdf_file_path:string) =
         let doc:PdfDocument  = new PdfDocument() 
         doc.LoadFromFile(pdf_file_path)
         let pages = doc.Pages
         seq { for page:PdfPageBase in pages do
                 let imgs =  page.ExtractImages()
                 for img:Image in imgs do
                    yield img}

     let extractExt(image:Image):ImageFormat = 
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

        if gif.SequenceEqual(bytes.Take(gif.Length)) then
            ImagesFormat.Gif 

//        if png.SequenceEqual(bytes.Take(png.Length)) then
//            ImageFormat.Png 
//
//        if tiff.SequenceEqual(bytes.Take(tiff.Length)) then
//            ImagesFormat.Tiff 
//
//        if  tiff2.SequenceEqual(bytes.Take(tiff2.Length)) then
//            ImagesFormat.Tiff 
//
//        if  jpeg.SequenceEqual(bytes.Take(jpeg.Length)) then
//            ImagesFormat.Jpeg 
//
//        if  jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)) then
//            ImagesFormat.Jpeg 

     ImagesFormat.Unknown 
      
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
     open System.Drawing.Imaging
 
//#region general methods
     let imageToByteArray(image:Image) =
        let ic = new ImageConverter()
        ic.ConvertTo(image, typeof<byte[]>):?>byte[]

     let byteArrayToImage(bytes:byte[]) =
         let ms:MemoryStream = new MemoryStream(bytes) 
         let image:Image = Image.FromStream(ms) 
         image

//#endregion


//#region playing with images

//(*
// * Usage example:
//*)
//let convertImage =
//    use newBitmap =
//        fromFile @"C:/Temp/with-color.jpg"
//        |> toRgbArray
//        |> toGrayScale // or custom in memory manipulation function
//        |> toBitmap
//    use savedBitmap =
//        toFile @"C:/Temp/gray-scale.png" newBitmap
//    ()
 
     // deserialize a bitmap file
     let fromFile (file_path:string) = new Bitmap(file_path)

     // serialize a bitmap as png
     let toPngFile (file_path: string) (bmp: Bitmap) =
        bmp.Save(file_path, Imaging.ImageFormat.Png) |> ignore
        bmp

     let toFile (file_path: string) (bmp: Bitmap) (format:Imaging.ImageFormat) =
        bmp.Save(file_path, format) |> ignore
        bmp

     // load a bitmap in array of tuples (x,y,Color)
     let toRgbArray (bmp : Bitmap) =
        [| for y in 0..bmp.Height-1 do
           for x in 0..bmp.Width-1 -> x,y,bmp.GetPixel(x,y) |]   

     // builds a bitmap instance from an array of tuples
     let toBitmap a =
        let height = (a |> Array.Parallel.map (fun (x,_,_) -> x) |> Array.max) + 1
        let width = (a |> Array.Parallel.map (fun (_,y,_) -> y) |> Array.max) + 1
        let bmp = new Bitmap(width, height)
        a |> Array.Parallel.iter (fun (x,y,c) -> bmp.SetPixel(x,y,c))
        bmp

     // converts an image to gray scale
     let toGrayScale a =
        a |> Array.Parallel.map (
            fun (x,y,c : System.Drawing.Color) -> 
                let gscale = int((float c.R * 0.3) + (float c.G * 0.59) + (float c.B * 0.11))
                in  x,y,Color.FromArgb(int c.A, gscale, gscale, gscale))

     // randomize or R or G or B each 1 to 3 pixels
     let randomColorize a =
        let rnd = new System.Random()
        let next = fun() -> rnd.Next(255)
        let rgb = function
            | 0 -> fun(c : System.Drawing.Color) -> Color.FromArgb(int c.A, next(), (int c.G), (int c.B))
            | 1 -> fun(c) -> Color.FromArgb(int c.A, (int c.R), next(), (int c.B))
            | _ -> fun(c) -> Color.FromArgb(int c.A, (int c.R), (int c.G), next())
        a |> Array.Parallel.mapi (
            fun i (x,y,c : System.Drawing.Color) -> 
                if i % rnd.Next(1,3) = 0 then x,y, rgb(rnd.Next(2))(c)
                else x,y,c)

     let crop (image:Image) (size:Size) =
          
     let setDPIonPng2(image:Image) = 
         use bitmap:Bitmap = new Bitmap(image) 
         use newBitmap:Bitmap  = new Bitmap(bitmap)
         newBitmap.SetResolution(300.f,300.f)
         newBitmap:>Image
 
     let setDPItoPng(image:Image) =
       // Get a PropertyItem from image1. Because PropertyItem does not
       // have public constructor, you first need to get existing PropertyItem
          let propItem:PropertyItem = image.GetPropertyItem(20624) 
          // Change the ID of the PropertyItem.
          propItem.Id <- 20625
          image.SetPropertyItem(propItem)
          image

     let setJpegResolution (file_path: string) (dpi:int byref) =
        use jpg = new FileStream(file_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None) 
        use br = new BinaryReader(jpg)   
        let mutable ok:bool = br.ReadUInt16() =  0xd8ffus       // Check header
        ok <- ok && br.ReadUInt16() = 0xe0ffus
        let z = br.ReadInt16()                       // Skip length
        ok  <- ok && br.ReadUInt32() = 0x4649464au // Should be JFIF
        ok  <- ok && br.ReadByte() =  0uy
        ok  <- ok && br.ReadByte() =  0x01uy           // Major version should be 1
        let y = br.ReadByte()                         // Skip minor version
        let density:byte  = br.ReadByte()
        ok  <- ok && (density = 1uy || density =  2uy)
        if (not ok) then raise (System.Exception("Not a valid JPEG file"))
        if (density = 2uy) then dpi <-  Convert.ToInt32(Math.Round((float dpi)/2.56))
        let bigendian = BitConverter.GetBytes(dpi)
        Array.Reverse(bigendian)
        jpg.Write(bigendian, 0, 2)
        jpg.Write(bigendian, 0, 2)
        let bitmap:Bitmap = new Bitmap(jpg)   
        bitmap:>Image 

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

    let augmentDpi = fun (i:Image) -> i:Image

    let scriviImgsFromPdf pdf_file_path dir_output_images augmentDpi = 
        let imgs = extractImages pdf_file_path
        for i:Image*string*ImagesFormat*int in imgs do
            let img,filename,ext,page = i
            let id = Guid.NewGuid()
            let auimg:Image = augmentDpi img
            auimg.Save(createFileName dir_output_images filename page id ext) |>ignore 
            
    let scriviImgsFromPdfsInDirectory dir_or_file_path dir_output_images augmentDpi = 
        if File.Exists(dir_or_file_path) then
           scriviImgsFromPdf dir_or_file_path dir_output_images augmentDpi
        elif Directory.Exists(dir_or_file_path) then  //presume is a directory
            let dir = new DirectoryInfo(dir_or_file_path)
            let files = dir.GetFiles("*.pdf", SearchOption.AllDirectories) //get only .pdf files
            let fullNames = files.Select(fun file -> file.FullName).ToArray();
            for pdf_file_path in fullNames do
                scriviImgsFromPdf pdf_file_path dir_output_images augmentDpi


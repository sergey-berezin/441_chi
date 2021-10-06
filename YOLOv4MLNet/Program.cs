using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace YOLOv4MLNet
{
    //https://towardsdatascience.com/yolo-v4-optimal-speed-accuracy-for-object-detection-79896ed47b50
    class Program
    {
        
        private static object BufferLock = new object();
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        const string modelPath = @"D:\model\yolov4.onnx";
        //string imageFolder = Console.ReadLine();
        //await Detector.DetectImage(imageFolder);
        //const string imageFolder = @"C:\Users\91930\Desktop\Net\441_ЦиЧжэнь\photo\Assets\Images";

        const string imageOutputFolder = @"C:\Users\91930\Desktop\Net\441_ЦиЧжэнь\photo\Assets\Output";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", 
                             "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe",
                             "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove",
                             "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange",
                             "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote",
                             "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        static async Task Main() //async 异步
        {
            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();
            Console.WriteLine("Write input path: ");
            string imageFolder = Console.ReadLine();
            
            // model is available here:
            // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
            var sw = new Stopwatch();
            sw.Start();
            await Task.Run(() => Parallel.ForEach(new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg", "animal.jpg","gongju.jpg" }, imageName => //await 等待 开启异步

            /*foreach (string imageName in new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg","animal.jpg" })
            {*/
             {
                   using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, imageName))))
                   {
                     // predict
                     lock (BufferLock) //lock 锁住，防止一个任务被多个进程抢
                     {
                         var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                         var results = predict.GetResults(classesNames, 0.3f, 0.7f);


                         using (var g = Graphics.FromImage(bitmap))
                         {
                             Console.Write("Find the following objects in the image "); Console.WriteLine(imageName, "\n");//打印图像名字

                             foreach (var res in results)
                             {
                                 // draw predictions
                                 var x1 = res.BBox[0];
                                 var y1 = res.BBox[1];
                                 var x2 = res.BBox[2];
                                 var y2 = res.BBox[3];
                                 lock (BufferLock)
                                 {
                                     g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                                 }
                                 using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                                 {
                                     lock (BufferLock)
                                     {
                                         g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                                     }
                                 }
                                 lock (BufferLock)
                                 {
                                     g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                                  new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                                 }
                                 
                                     Console.WriteLine(res.Label + " " + res.Confidence.ToString("0.00"), " ");//打印该图像中物体
                                 
                             };
                             bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(imageName, "_processed" + Path.GetExtension(imageName))));

                         }
                     }
                   }
               }));

            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");


        }
    }
}

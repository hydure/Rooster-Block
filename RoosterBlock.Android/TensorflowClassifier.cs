﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Android.App;
using Android.Graphics;
using Java.IO;
using Java.Nio;
using Java.Nio.Channels;

namespace RoosterBlock.Droid
{
    public class TensorflowClassifier
    {
        //FloatSize is a constant with the value of 4 because a float value is 4 bytes
        const int FloatSize = 4;
        //PixelSize is a constant with the value of 3 because a pixel has three color channels: Red Green and Blue
        const int PixelSize = 3;

        public List<ImageClassificationModel> Classify(byte[] image)
        {
            MappedByteBuffer mappedByteBuffer = GetModelAsMappedByteBuffer();
            Xamarin.TensorFlow.Lite.Interpreter interpreter = new Xamarin.TensorFlow.Lite.Interpreter(mappedByteBuffer);

            //To resize the image, we first need to get its required width and height
            Xamarin.TensorFlow.Lite.Tensor tensor = interpreter.GetInputTensor(0);
            int[] shape = tensor.Shape();

            int width = shape[1];
            int height = shape[2];

            ByteBuffer byteBuffer = GetPhotoAsByteBuffer(image, width, height);

            //use StreamReader to import the labels from labels.txt
            StreamReader streamReader = new StreamReader(Application.Context.Assets.Open("labels.txt"));

            //Transform labels.txt into List<string>
            List<string> labels = streamReader.ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            //Convert our two-dimensional array into a Java.Lang.Object, the required input for Xamarin.TensorFlow.List.Interpreter
            float[][] outputLocations = new float[1][] { new float[labels.Count] };
            Java.Lang.Object outputs = Java.Lang.Object.FromArray(outputLocations);

            interpreter.Run(byteBuffer, outputs);
            float[][] classificationResult = outputs.ToArray<float[]>();

            //Map the classificationResult to the labels and sort the result to find which label has the highest probability
            List<ImageClassificationModel> classificationModelList = new List<ImageClassificationModel>();

            for (int i = 0; i < labels.Count; i++)
            {
                string label = labels[i]; classificationModelList.Add(new ImageClassificationModel(label, classificationResult[0][i]));
            }

            return classificationModelList;
        }

        //Convert model.tflite to Java.Nio.MappedByteBuffer , the require type for Xamarin.TensorFlow.Lite.Interpreter
        private MappedByteBuffer GetModelAsMappedByteBuffer()
        {
            var assetDescriptor = Application.Context.Assets.OpenFd("model.tflite");
            var inputStream = new FileInputStream(assetDescriptor.FileDescriptor);

            var mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);

            return mappedByteBuffer;
        }

        //Resize the image for the TensorFlow interpreter
        private ByteBuffer GetPhotoAsByteBuffer(byte[] image, int width, int height)
        {
            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
            var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);

            var modelInputSize = FloatSize * height * width * PixelSize;
            var byteBuffer = ByteBuffer.AllocateDirect(modelInputSize);
            byteBuffer.Order(ByteOrder.NativeOrder());

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;

            //Loop through each pixels to create a Java.Nio.ByteBuffer
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];

                    byteBuffer.PutFloat(pixelVal >> 16 & 0xFF);
                    byteBuffer.PutFloat(pixelVal >> 8 & 0xFF);
                    byteBuffer.PutFloat(pixelVal & 0xFF);
                }
            }

            bitmap.Recycle();

            return byteBuffer;
        }
    }
}
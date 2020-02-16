using System;
using NUnit.Framework;
using Google.Cloud.Vision.V1;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GoogleObjectAnnotationTests
{
    [TestFixture]
    public class ObjectAnnotationTests
    {
        [Test]
        [Timeout(60 * 1000)]
        public void GoogleAnnotator_StreetItems_ExpectedItems()
        {
            // Arrange
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromUri("https://cloud.google.com/vision/docs/images/bicycle_example.png");

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = client.DetectLocalizedObjects(image, maxResults: 10);
            WriteDurationToFile("TestDuration_StreetItems", stopwatch.ElapsedMilliseconds);

            // Assert
            Console.WriteLine($"Response is {response.ToString()}");
            Assert.AreEqual(4, response.Count);
            var itemsNames = response.Select(item => item.Name);

            //"Tire", "Door
            string[] expectedNames = { "Bicycle wheel", "Bicycle", "Bicycle wheel", "Picture frame" };
            CollectionAssert.AreEquivalent(expectedNames, itemsNames);

            foreach (var item in response)
            {
                Assert.GreaterOrEqual(item.Score, 0.4);
            }
        }

        [Test]
        [Timeout(60 * 1000)]
        public void GoogleAnnotator_ItemInLightFog_AtLeastOneCar()
        {
            // Arrange
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromUri("https://www.powerbulbs.com/uploads/images/blog_images/Halogen-vs-LED-Fog-Lights-1.jpg");

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = client.DetectLocalizedObjects(image, maxResults: 10);
            WriteDurationToFile("TestDuration_ItemInLightFog", stopwatch.ElapsedMilliseconds);

            // Assert
            Console.WriteLine($"Response is {response.ToString()}");
            Assert.GreaterOrEqual(response.Count,1);

            var itemsNames = response.Select(item => item.Name);

            CollectionAssert.Contains(itemsNames, "Car");

            foreach (var item in response)
            {
                Assert.GreaterOrEqual(item.Score, 0.4);
            }
        }

        [Test]
        [Timeout(60 * 1000)]
        public void GoogleAnnotator_BlackImage_NoItems()
        {
            // Arrange
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromUri("https://fsb.zobj.net/crop.php?r=CGgklARGrdK-BSeIap2tk_QyraRJmZkZ6dKI8wWaX7zEP0zEChNdn8z9wxmchxgRkEr84tySNzWKYFU28DVmyBIGBO9WcVQWv1YA9tdUiHVBbgUJ2B_aBKIi5TnU1sGk2IlKo9aza_Vq1-STaKCKcasaLzD7H8a6CUEeBDnLOqp1eBxReyolQhzmwFlB8uzNzC1qrrPIqZye9WR_");

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = client.DetectLocalizedObjects(image, maxResults: 10);
            WriteDurationToFile("TestDuration_BlackImage", stopwatch.ElapsedMilliseconds);

            // Assert
            Console.WriteLine($"Response is {response.ToString()}");

            CollectionAssert.IsEmpty(response);

            foreach (var item in response)
            {
                Assert.GreaterOrEqual(item.Score, 0.4);
            }
        }

        [Test]
        [Timeout(60 * 1000)]
        public void GoogleAnnotator_Thread_Parallel()
        {
            var client = ImageAnnotatorClient.Create();
            string[] imagesUrls = {
            "https://www.powerbulbs.com/uploads/images/blog_images/Halogen-vs-LED-Fog-Lights-1.jpg",
            "https://cloud.google.com/vision/docs/images/bicycle_example.png"
		};

            // Act
            var stopwatch = Stopwatch.StartNew();
              Parallel.ForEach(imagesUrls, url => {
             var image = Image.FromUri(url);
               client.DetectLocalizedObjects(image, maxResults: 10);
            });

            WriteDurationToFile("TestDuration_Parallel", stopwatch.ElapsedMilliseconds);
        }

        private void WriteDurationToFile(string testName, double elapsedMilliseconds)
        {
            var row = $"{DateTime.Now.ToString()}\t{elapsedMilliseconds}\n";
            File.AppendAllText($@"{testName}.txt", row);
        }
    }
}
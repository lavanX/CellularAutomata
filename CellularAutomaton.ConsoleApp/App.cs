using System;
using System.IO;
using CellularAutomaton.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;

namespace CellularAutomaton.ConsoleApp
{
    internal class App
    {
        public void Run()
        {
            int size = Ask("Размер матрицы: ", i => i > 200, "Размер матрицы должен быть больше или равен 200.");
            int rule = Ask("Правило (0-1023): ", i => i > 0 && i <= 1024,
                "Правило должно быть больше 0 и меньше 1023 включительно.");
            int initialX = Ask($"Начальное значение X (0-{size}): ", i => i > 0 && i < size,
                $"X должен быть больше или равен 0 и меньше {size}.");
            int initialY = Ask($"Начальное значение Y (0-{size}): ", i => i > 0 && i < size,
                $"Y должен быть больше или равен 0 и меньше {size}.");
            int iterationsCount = Ask("Кол-во итераций: ", i => i > 0, "Количество итераций должно быть больше 0.");

            ICrystal crystal = CrystalFactory.CreateCrystal(size, rule);

            crystal.SetState(initialX, initialY, PixelState.On);

            Console.WriteLine("Вычисление состояния матрицы...");
            crystal.UpdateMatrixState(iterationsCount);
            Image<Rgba32> image = CreateImage(crystal);

            Console.WriteLine("Вычисления завершены.");
            Console.WriteLine("Введите путь фала для сохранения картинки (*.bmp): ");
            var path = Console.ReadLine();
            while (!TrySaveImage(path, image))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Не удалось сохранить картинку. Убедитесь в правильности введенного пути."); 
                Console.ResetColor();
                Console.WriteLine("Введите путь фала для сохранения картинки (*.bmp): ");
                path = Console.ReadLine();
            }
        }

        private bool TrySaveImage(string path, Image<Rgba32> image)
        {
            if(image == null)
                throw new ArgumentNullException(nameof(image));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            try
            {
                if (!path.EndsWith(".bmp"))
                    path += ".bmp";
                
                image.Save(path);
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }


        int Ask(string question, Func<int, bool> validator, string validatorMessage = "")
        {
            Console.Write(question);
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int result))
                {
                    if(validator?.Invoke(result) ?? true)
                        return result;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(validatorMessage);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Введите число!");
                }
                Console.ResetColor();
                Console.Write(question);

            }
        }
        private Image<Rgba32> CreateImage(ICrystal crystal)
        {
            if(crystal is null)
                throw new ArgumentNullException(nameof(crystal));

            Image<Rgba32> image = new Image<Rgba32>(crystal.Size, crystal.Size);
            for (int column = 0; column < crystal.Size; column++)
            {
                for (int row = 0; row < crystal.Size; row++)
                {
                    Rgba32 color = crystal.GetState(column, row) switch
                    {
                        PixelState.Off => new Rgba32(0, 0, 0, 255),//Black
                        PixelState.On => new Rgba32(255, 255, 255, 255), //White
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    image[column, row] = color;
                }
            }

            return image;
        }
    }
}
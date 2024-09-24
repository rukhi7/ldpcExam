using Microsoft.VisualBasic;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace dipLdpc
{
	[StructLayout(LayoutKind.Sequential)]
	// Структура, описывающая заголовок WAV файла.
	internal class WavHeader
	{
		// WAV-формат начинается с RIFF-заголовка:

		// Содержит символы "RIFF" в ASCII кодировке
		// (0x52494646 в big-endian представлении)
		public UInt32 ChunkId;

		// 36 + subchunk2Size, или более точно:
		// 4 + (8 + subchunk1Size) + (8 + subchunk2Size)
		// Это оставшийся размер цепочки, начиная с этой позиции.
		// Иначе говоря, это размер файла - 8, то есть,
		// исключены поля chunkId и chunkSize.
		public UInt32 ChunkSize;

		// Содержит символы "WAVE"
		// (0x57415645 в big-endian представлении)
		public UInt32 Format;

		// Формат "WAVE" состоит из двух подцепочек: "fmt " и "data":
		// Подцепочка "fmt " описывает формат звуковых данных:

		// Содержит символы "fmt "
		// (0x666d7420 в big-endian представлении)
		public UInt32 Subchunk1Id;

		// 16 для формата PCM.
		// Это оставшийся размер подцепочки, начиная с этой позиции.
		public UInt32 Subchunk1Size;

		// Аудио формат, полный список можно получить здесь http://audiocoding.ru/wav_formats.txt
		// Для PCM = 1 (то есть, Линейное квантование).
		// Значения, отличающиеся от 1, обозначают некоторый формат сжатия.
		public UInt16 AudioFormat;

		// Количество каналов. Моно = 1, Стерео = 2 и т.д.
		public UInt16 NumChannels;

		// Частота дискретизации. 8000 Гц, 44100 Гц и т.д.
		public UInt32 SampleRate;

		// sampleRate * numChannels * bitsPerSample/8
		public UInt32 ByteRate;

		// numChannels * bitsPerSample/8
		// Количество байт для одного сэмпла, включая все каналы.
		public UInt16 BlockAlign;

		// Так называемая "глубиная" или точность звучания. 8 бит, 16 бит и т.д.
		public UInt16 BitsPerSample;

		// Подцепочка "data" содержит аудио-данные и их размер.

		// Содержит символы "data"
		// (0x64617461 в big-endian представлении)
		public UInt32 Subchunk2Id;

		// numSamples * numChannels * bitsPerSample/8
		// Количество байт в области данных.
		public UInt32 Subchunk2Size;

		// Далее следуют непосредственно Wav данные.
	}

	class RunMain
	{
		public static void WavReaderMain()
		{
			var header = new WavHeader();
			// Размер заголовка
			var headerSize = Marshal.SizeOf(header);
			var fileName = @"C:\audio\aac_audio.wav";
            FileStream fileStream = null;
            try
			{
				fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			}
			catch(Exception ex)
			{
				string message;
				string caption;

                if (ex.HResult < 0)
				{
                    message = $"hardcoded file {fileName} doesn't exist!";
                    caption = "File access error";
				}
				else
				{
                    message = $"uknown error while opening hardcoded file {fileName}";
                    caption = "File access error";
                }
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                System.Windows.Forms.MessageBox.Show(message, caption, buttons);
				return;

            }
            var buffer = new byte[headerSize];
			fileStream.Read(buffer, 0, headerSize);

			// Чтобы не считывать каждое значение заголовка по отдельности,
			// воспользуемся выделением unmanaged блока памяти
			var headerPtr = Marshal.AllocHGlobal(headerSize);
			// Копируем считанные байты из файла в выделенный блок памяти
			Marshal.Copy(buffer, 0, headerPtr, headerSize);
			// Преобразовываем указатель на блок памяти к нашей структуре
			Marshal.PtrToStructure(headerPtr, header);

			// Выводим полученные данные
			WpfApp1.MainWindow.printOutput($"Sample rate: {header.SampleRate}" );
			WpfApp1.MainWindow.printOutput($"Channels: {header.NumChannels}");
			WpfApp1.MainWindow.printOutput($"Bits per sample: {header.BitsPerSample}");

			// Посчитаем длительность воспроизведения в секундах
			var durationSeconds = 1.0 * header.Subchunk2Size / (header.BitsPerSample / 8.0) / header.NumChannels / header.SampleRate;
			var durationMinutes = (int)Math.Floor(durationSeconds / 60);
			durationSeconds = durationSeconds - (durationMinutes * 60);
			WpfApp1.MainWindow.printOutput($"Duration: {durationMinutes, 0:00}:{durationSeconds, 1:00}");

//			Console.ReadKey();

			// Освобождаем выделенный блок памяти
			Marshal.FreeHGlobal(headerPtr);
		}
	}
}

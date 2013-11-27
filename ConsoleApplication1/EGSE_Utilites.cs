/*** EDGE_Utilites.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль дополнительных утилит для КИА
**
** Author: Семенов Александр, Мурзин Святослав
** Project: КИА
** Module: EDGE UTILITES
** Requires: 
** Comments:
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
**
*/

using System;
using System.Runtime.InteropServices;

namespace EGSE.UTILITES
{
    /// <summary>
    /// Класс кольцевого буфера
    /// </summary>
    class CBuf
    {
        public uint curReadPos;
        public uint curWritePos;

        public CBuf(uint bufSize)
        {

        }
    }

    /// <summary>
    /// Класс рассчета значений по калибровочным данным
    /// </summary>
    class Measurements
    {
        public Measurements()
        {

        }

        ~Measurements()
        {

        }

        public bool loadFromFile(string fName)
        {
            return true;
        }

        public bool addData(float x, float y)
        {
            return true;
        }

        public bool getData(float val, ref float res) {
            res = 0;
            return true;
        }
    }

    /// <summary>
    /// Эта структура позволяет подсчитать скорость выполнения кода одним из
    /// наиболее точным способов. Фактически вычисления производятся в тактах
    /// процессора, а потом переводятся в милисекунд (десятичная часть 
    /// является долями секунды).
    /// </summary>
    public struct PerfCounter
    {
       Int64 _start;

       /// <summary>
       /// Начинает подсчет вермени выполнения.
       /// </summary>
       public void Start()
       {
           _start = 0;
           QueryPerformanceCounter(ref _start);
       }

        /// <summary>
        /// Завершает полсчет вермени исполнения и возвращает время в секундах.
        /// </summary>
        /// <returns>Время в секундах потраченое на выполнение участка
        /// кода. Десятичная часть отражает доли секунды.</returns>
        public float Finish()
        {
           Int64 finish = 0;
           QueryPerformanceCounter(ref finish);

           Int64 freq = 0;
           QueryPerformanceFrequency(ref freq);
           return (((float)(finish - _start) / (float)freq));
        }
        [DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceCounter(ref Int64 performanceCount);

        [DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceFrequency(ref Int64 frequency);
    }
}

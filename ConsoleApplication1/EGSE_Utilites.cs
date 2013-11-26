/*** EDGE_Utilites.cs
**
** (с) 2013 Семенов Александр, ИКИ РАН
 *
 * Модуль дополнительных утилит для КИА
**
** Author: Семенов Александр
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
}

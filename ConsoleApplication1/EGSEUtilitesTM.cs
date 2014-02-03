//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesTM.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Utilites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Значение телеметрического параметра, изменения которого необходимо отслеживать 
    /// (обычно для логгирования изменения состояния - контактные датчики, включени питания и т.д.)
    /// Пример использования:
    /// static void tFunc(int val)
    /// {
    ///     string s = String.Empty;
    ///     if ((val and 1) == 1) s += "ПК1 ВКЛ ";
    ///     if ((val and 2) == 2) s += "ПК2 ВКЛ ";
    ///     System.Console.WriteLine(s);
    /// }
    /// TMValue val1 = new TMValue(0, tFunc, true);
    /// val1.SetVal(3);
    /// </summary>
    public class TMValue
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TMValue" />.
        /// </summary>
        public TMValue()
        {
            Value = -1;
            MakeTest = false;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TMValue" />.
        /// Параметр телеметрии.
        /// </summary>
        /// <param name="val">Значение параметра</param>
        /// <param name="func">Функция при изменении параметра, можно передать null</param>
        /// <param name="makeTest">Нужно ли сравнивать старое и новое значение параметра</param>
        public TMValue(int val, ChangeValueEventHandler func, bool makeTest)
        {
            Value = val;
            ChangeValueEvent = func;
            MakeTest = makeTest;
        }

        /// <summary>
        /// Описание делегата функции, которую нужно вызвать при изменении параметра.
        /// </summary>
        /// <param name="val">Изменяемое значение параметра</param>
        public delegate void ChangeValueEventHandler(int val);

        /// <summary>
        /// Получает или задает делагата на изменение параметра.
        /// </summary>
        public ChangeValueEventHandler ChangeValueEvent { get; set; }

        /// <summary>
        /// Получает или задает значение параметра.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, нужно ли проверять параметр на изменение значения.
        /// </summary>
        public bool MakeTest { get; set; }

        /// <summary>
        /// Присваивание значения, если необходима проверка значения и определена функция проверки.
        /// </summary>
        /// <param name="val">Новое значение</param>
        public void SetVal(int val)
        {
            bool _changed = true;
            if (MakeTest)
            {
                _changed = Value != val;
            }

            if (_changed)
            {
                ChangeValueEvent(val);
            }

            Value = val;
        }
    }
}

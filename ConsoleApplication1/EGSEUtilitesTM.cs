using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGSE.Utilites
{
    /// <summary>
    /// Значение телеметрического параметра, изменения которого необходимо отслеживать (обычно для логгирования изменения состояния - контактные датчики, включени питания и т.д.)
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
        /// Описание делегата функции, которую нужно вызвать при изменении параметра
        /// </summary>
        /// <param name="val"></param>
        public delegate void ChangeValueEventHandler(int val);

        /// <summary>
        /// Делагат на изменение параметра
        /// </summary>
        public ChangeValueEventHandler ChangeValueEvent;

        /// <summary>
        /// Значение параметра
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Нужно ли проверять параметр на изменение значения
        /// </summary>
        public bool MakeTest { get; set; }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public TMValue()
        {
            Value = -1;
            MakeTest = false;
        }

        /// <summary>
        /// Создаем параметр сразу 
        /// </summary>
        /// <param name="val">Значение</param>
        /// <param name="fun">Функция при изменении параметра, можно передать null</param>
        /// <param name="makeTest">Нужно ли сравнивать старое и новое значение параметра</param>
        public TMValue(int val, ChangeValueEventHandler fun, bool makeTest)
        {
            Value = val;
            ChangeValueEvent = fun;
            MakeTest = makeTest;
        }

        /// <summary>
        /// Присваивание значения
        /// Если необходима проверка значения и определена функция проверки
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

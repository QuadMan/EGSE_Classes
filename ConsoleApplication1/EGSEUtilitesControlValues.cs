namespace EGSE.Utilites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Класс, позволяющий отслеживать изменения значений, которые влияют на отображение интерфейса
    /// (параметры имитаторов, значения включения полукомплектов и т.д.) и которые может устанавливать пользователь
    /// Пример работы:
    /// ControlValue HSIParameters;
    /// при получении данных от устройства, делаем HSIParameters.GetValue = value;
    /// при изменении пользователем интерфейса, делаем HSIParameters.SetValue = value;
    /// в таймере, вызываемом 2 раза в секунду, выполняем проверку
    /// if (HSIParameters.TimerTick() == ControlValue.ValueState.vsChanged) {
    /// нужно обновить экран, так как значение, пришедшее от прибора не совпадает с установленным
    /// }
    /// </summary>
    public class ControlValue
    {
        private const int UPDATE_TIMEOUT_TICKS = 3;
        public delegate void setFunctionDelegate(UInt32 value);

        class CVProperty
        {
            public int Idx;
            public UIElement Control;
            public UInt16 BitIdx;
            public UInt16 BitLen;
            public setFunctionDelegate Func;

            public CVProperty(int _idx, UIElement _cb, UInt16 _bitIdx, UInt16 _bitLen, setFunctionDelegate _func)
            {
                Idx = _idx;
                Control = _cb;
                BitIdx = _bitIdx;
                BitLen = _bitLen;
                Func = _func;
            }
        };

        List<CVProperty> cvpl = new List<CVProperty>();

        private enum ValueState { vsUnchanged, vsChanged, vsCounting };
        // старое значение, которое получили
        private int _oldGetValue;
        // значение, которое получаем из USB
        private int _getValue;
        // значение, которое уставливается из интерфейса
        private int _setValue;
        // значение счетчика времени до проверки совпадения GetValue и SetValue
        private int _timerCnt;
        //
        private int _defaultValue;
        //
        private bool _updatingGetState;
        //
        private bool _updateUI;

        public bool UpdateUI
        {
            get { return _updateUI; }
            set { _updateUI = value; }
        }

        public ControlValue(int defaultValue = 0)
        {
            _oldGetValue = -1;
            _getValue = 0;
            _setValue = 0;
            _timerCnt = 0;
            _updatingGetState = false;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_idx"></param>
        /// <param name="_cb">null тоже можно передавать</param>
        /// <param name="_bitIdx"></param>
        /// <param name="_bitLen"></param>
        /// <param name="_func"></param>
        /// <returns></returns>
        public bool AddProperty(int _idx, UIElement _cb, UInt16 _bitIdx, UInt16 _bitLen, setFunctionDelegate _func)
        {
            if (_cb is CheckBox)
            {
                (_cb as CheckBox).Click += _cb_Click;
            }
            else if (_cb is ComboBox)
            {
                (_cb as ComboBox).SelectionChanged += ControlValue_SelectionChanged;
            }
            //else return false;

            //TODO: проверить, что такого индекса еще нет
            cvpl.Add(new CVProperty(_idx, _cb, _bitIdx, _bitLen, _func));
            return true;
        }

        private bool setCVProperty(CVProperty cv, int pValue, bool autoSendValue = true)
        {
            if (cv == null) return false;

            Int32 mask = 0;
            for (UInt16 i = 0; i < cv.BitLen; i++)
            {
                mask |= (1 << (cv.BitIdx + i));
            }
            pValue &= (mask >> cv.BitIdx);
            _setValue &= ~mask;
            _setValue |= pValue << cv.BitIdx;
            if (autoSendValue)
            {
                TimerSet(UPDATE_TIMEOUT_TICKS);
                cv.Func((uint)_setValue);
            }

            return true;
        }

        public bool SetProperty(int pIdx, int pValue, bool autoSendValue = true)
        {
            CVProperty cv = cvpl.Find(p => p.Idx == pIdx);

            return setCVProperty(cv, pValue, autoSendValue);
        }

        void ControlValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingGetState) return;
            CVProperty cv = cvpl.Find(p => (p.Control is ComboBox) && (p.Control == sender));

            setCVProperty(cv, (cv.Control as ComboBox).SelectedIndex);
        }

        void _cb_Click(object sender, RoutedEventArgs e)
        {
            if (_updatingGetState) return;

            CVProperty cv = cvpl.Find(p => (p.Control is CheckBox) && (p.Control == sender));

            setCVProperty(cv, Convert.ToInt32((cv.Control as CheckBox).IsChecked));
        }

        /// <summary>
        /// Метод для принудительного вызова через определнное время проверки
        /// установленного и полученного значений
        /// </summary>
        public void RefreshGetValue()
        {
            _timerCnt = UPDATE_TIMEOUT_TICKS;
        }

        /// <summary>
        /// Значение, полученное из USB
        /// </summary>
        public int GetValue
        {
            get
            {
                return _getValue;
            }
            set
            {
                _oldGetValue = _getValue;
                _getValue = value;
            }
        }

        /// <summary>
        /// Значение, установленное из интерфейса
        /// </summary>
        public int SetValue
        {
            get { return _setValue; }
            set
            {
                _setValue = value;

                TimerSet(UPDATE_TIMEOUT_TICKS);            // значение установили, ждем 2 секунды до проверки Get и SetValue
            }
        }

        /// <summary>
        /// Устаналиваем, сколько "тиков" ждать
        /// </summary>
        /// <param name="timerVal"></param>
        private void TimerSet(int timerVal)
        {
            _timerCnt = timerVal;
        }

        /// <summary>
        /// Один тик таймера (вызывается из внешнего прерывания и может быть любым, хоть 1 секунда, хоть 500 мс)
        /// </summary>
        /// <returns>Функция возвращает результат проверки Set и Get значений, если время истекло, или vsCounting, если счет продолжается</returns>
        public void TimerTick()
        {
            if ((_timerCnt > 0) && (--_timerCnt == 0))          // пришло время для проверки Get и Set Value
            {
                if (Changed())
                {
                    _updateUI = false;
                    UpdateGetProperties();
                }
            }
        }

        private void UpdateGetProperties()
        {
            _updatingGetState = true;
            foreach (CVProperty cv in cvpl)
            {
                if (cv.Control is CheckBox)
                {
                    (cv.Control as CheckBox).IsChecked = ((_getValue & (1 << cv.BitIdx)) > 0);
                }
                if (cv.Control is ComboBox)
                {
                    Int32 mask = 0;
                    for (UInt16 i = 0; i < cv.BitLen; i++)
                    {
                        mask |= (1 << (cv.BitIdx + i));
                    }
                    (cv.Control as ComboBox).SelectedIndex = (_getValue & mask) >> cv.BitIdx;
                }
            }
            _setValue = _getValue;
            _updatingGetState = false;
        }

        /// <summary>
        /// Проверяем значения
        /// </summary>
        /// <returns></returns>
        private bool Changed()
        {
            return (_oldGetValue != _getValue) || (_setValue != _getValue) || (_updateUI);
        }
    }

}

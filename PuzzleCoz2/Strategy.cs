using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Puzzle
{
    internal enum Mantiksal
    {
        BoslugaOyna,
        SiracilOyun
    }

    internal enum Yonler
    {
        Sol,
        Sag,
        Yukari,
        Assagi,
    }

    internal sealed class State : IComparable
    {
        private int[] mNodes;
        private int mSpaceIndex;
        private string mStateCode;
        private int mCostf;
        private int mCosth;
        private int mCostg;
        private Mantiksal _mMantiksal;
        private State mParent;

        internal State(State parent, int[] nodes, Mantiksal mantiksal)
        {
            mNodes = nodes;
            mParent = parent;
            _mMantiksal = mantiksal;
            Hesapla();
            mStateCode = StateHesap();
        }

        private State(State parent, int[] nodes)
        {
            mNodes = nodes;
            mParent = parent;
            _mMantiksal = parent._mMantiksal;
            Hesapla();
            mStateCode = StateHesap();
        }

        public override bool Equals(object obj)
        {
            State that = obj as State;

            return that != null && this.mStateCode.Equals(that.mStateCode);
        }

        public override int GetHashCode()
        {
            return mStateCode.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            State that = obj as State;

            if (that != null)
            {
                return (this.mCostf).CompareTo(that.mCostf);
            }

            return 0;
        }

        public bool IsCostlierThan(State thatState)
        {
            return this.mCostg > thatState.mCostg;
        }

        public String GetStateCode()
        {
            return mStateCode;
        }

        private void Hesapla()
        {
            if (mParent == null)
            {
                mCostg = 0;
            }
            else
            {
                mCostg = mParent.mCostg + 1;
            }

            mCosth = MantiksalDeger();

            mCostf = mCosth + mCostg;
        }

        private int MantiksalDeger()
        {
            if (_mMantiksal == Mantiksal.SiracilOyun)
            {
                return MerkezMesafeDegeri();
            }
            else
            {
                return GetMisplacedTilesCost();
            }
        }

        private int GetMisplacedTilesCost()
        {
            int heuristicCost = 0;

            for (int i = 0; i < mNodes.Length; i++)
            {
                int value = mNodes[i] - 1;

                // Boşluk değeri -1
                if (value == -2)
                {
                    value = mNodes.Length - 1;
                    mSpaceIndex = i;
                }

                if (value != i)
                {
                    heuristicCost++;
                }
            }

            return heuristicCost;
        }

        private int MerkezMesafeDegeri()
        {
            int mantiksalDeger = 0;
            int gridX = (int)Math.Sqrt(mNodes.Length);
            int idealX;
            int idealY;
            int guncelX;
            int guncelY;
            int deger;

            for (int i = 0; i < mNodes.Length; i++)
            {
                deger = mNodes[i] - 1;
                if (deger == -2)
                {
                    deger = mNodes.Length - 1;
                    mSpaceIndex = i;
                }

                if (deger != i)
                {
                    idealX = deger % gridX;
                    idealY = deger / gridX;

                    guncelX = i % gridX;
                    guncelY = i / gridX;

                    mantiksalDeger += (Math.Abs(idealY - guncelY) + Math.Abs(idealX - guncelX));
                }
            }

            return mantiksalDeger;
        }

        private String StateHesap()
        {
            StringBuilder code = new StringBuilder();

            for (int i = 0; i < mNodes.Length; i++)
            {
                code.Append(mNodes[i] + "*");
            }

            return code.ToString().Trim(new char[] { '*' });
        }

        public int[] GetState()
        {
            int[] state = new int[mNodes.Length];
            Array.Copy(mNodes, state, mNodes.Length);

            return state;
        }

        public bool IsFinalState() // Hepsi Doğruysa buraya akıyoz
        {
            return mCosth == 0;
        }

        public State GetParent()
        {
            return mParent;
        }

        public List<State> SiradakiYerler(ref List<State> siradakiYerler)
        {
            siradakiYerler.Clear();
            State yer;

            foreach (Yonler yon in Enum.GetValues(typeof(Yonler)))
            {
                yer = SiradakiYeriBul(yon);

                if (yer != null)
                {
                    siradakiYerler.Add(yer);
                }
            }

            return siradakiYerler;
        }

        private State SiradakiYeriBul(Yonler yonler)
        {
            int pozisyon;

            if (HareketKon(yonler, out pozisyon))
            {
                int[] nodes = new int[mNodes.Length];
                Array.Copy(mNodes, nodes, mNodes.Length);

                Degistir(nodes, mSpaceIndex, pozisyon);

                return new State(this, nodes);
            }

            return null;
        }

        private void Degistir(int[] nodes, int i, int j)
        {
            int t = nodes[i];
            nodes[i] = nodes[j];
            nodes[j] = t;
        }

        private bool HareketKon(Yonler yonler, out int yeniPozisyon)
        {
            int yeniX = -1;
            int yeniY = -1;
            int gridX = (int)Math.Sqrt(mNodes.Length);
            int guncelX = mSpaceIndex % gridX;
            int guncelY = mSpaceIndex / gridX;
            yeniPozisyon = -1;

            switch (yonler)
            {
                case Yonler.Yukari:
                    {
                        if (guncelY != 0)
                        {
                            yeniX = guncelX;
                            yeniY = guncelY - 1;
                        }
                    }
                    break;

                case Yonler.Assagi:
                    {
                        if (guncelY < (gridX - 1))
                        {
                            yeniX = guncelX;
                            yeniY = guncelY + 1;
                        }
                    }
                    break;

                case Yonler.Sol:
                    {
                        if (guncelX != 0)
                        {
                            yeniX = guncelX - 1;
                            yeniY = guncelY;
                        }
                    }
                    break;

                case Yonler.Sag:
                    {
                        if (guncelX < (gridX - 1))
                        {
                            yeniX = guncelX + 1;
                            yeniY = guncelY;
                        }
                    }
                    break;
            }

            if (yeniX != -1 && yeniY != -1)
            {
                yeniPozisyon = yeniY * gridX + yeniX;
            }

            return yeniPozisyon != -1;
        }

        public override string ToString()
        {
            return "State:" + mStateCode + ", g:" + mCostg + ", h:" + mCosth + ", f:" + mCostf;
        }
    }

    internal delegate void StateChanged(int[] currentState, bool isFinal);
    internal delegate void Cozum(int steps, int time, int stateExamined);

    internal sealed class MantikPuzzle
    {
        #region Fields

        private Stopwatch mStopWatch;
        internal event StateChanged KonumDegismesi;
        internal event Cozum OnCozum;

        #endregion Fields

        #region Methods

        internal MantikPuzzle()
        {
            mStopWatch = new Stopwatch();
        }

        internal void Coz(int[] nodes, Mantiksal mantiksal)
        {
            ThreadPool.QueueUserWorkItem(item => Start(nodes, mantiksal));
        }

        private void Start(int[] nodes, Mantiksal mantiksal)
        {
            int acikYer;
            int yerSayisi = -1;
            State guncelYer = null;
            List<State> nextYer = new List<State>();
            HashSet<String> serbestYer = new HashSet<string>();
            MinPriorityQueue<State> serbestYerSira = new MinPriorityQueue<State>(nodes.Length * 3);
            Dictionary<String, State> kapaliYer = new Dictionary<string, State>(nodes.Length * 3);

            State state = new State(null, nodes, mantiksal);
            serbestYerSira.Enqueue(state);
            serbestYer.Add(state.GetStateCode());

            ZamaniBaslat();

            while (!serbestYerSira.IsEmpty())
            {
                guncelYer = serbestYerSira.Dequeue();
                serbestYer.Remove(guncelYer.GetStateCode());

                yerSayisi++;

                if (guncelYer.IsFinalState())
                {
                    ZamaniDurdur(yerSayisi);
                    break;
                }

                guncelYer.SiradakiYerler(ref nextYer);

                if (nextYer.Count > 0)
                {
                    State kapali;
                    State acik;
                    State siradaki;

                    for (int i = 0; i < nextYer.Count; i++)
                    {
                        kapali = null;
                        acik = null;
                        siradaki = nextYer[i];

                        if (serbestYer.Contains(siradaki.GetStateCode()))
                        {
                            acik = serbestYerSira.Find(siradaki, out acikYer);

                            if (acik.IsCostlierThan(siradaki))
                            {
                                serbestYerSira.Kaldir(acikYer);
                                serbestYerSira.Enqueue(siradaki);
                            }
                        }
                        else
                        {
                            String stateCode = siradaki.GetStateCode();

                            if (kapaliYer.TryGetValue(stateCode, out kapali))
                            {
                                if (kapali.IsCostlierThan(siradaki))
                                {
                                    kapaliYer.Remove(stateCode);
                                    kapaliYer[stateCode] = siradaki;
                                }
                            }
                        }

                        if (acik == null && kapali == null)
                        {
                            serbestYerSira.Enqueue(siradaki);
                            serbestYer.Add(siradaki.GetStateCode());
                        }
                    }

                    kapaliYer[guncelYer.GetStateCode()] = guncelYer;
                }
            }

            if (guncelYer != null && !guncelYer.IsFinalState()) //Çözüm Bulunamadı
            {
                guncelYer = null;
            }

            Cozulunce(guncelYer, yerSayisi);
            SonAsama(guncelYer);
        }

        private void ZamaniBaslat()
        {
            mStopWatch.Reset();
            mStopWatch.Start();
        }

        private void ZamaniDurdur(int stateCount)
        {
            mStopWatch.Stop();
        }

        private void SonAsama(State state)
        {
            if (state != null)
            {
                Stack<State> path = new Stack<State>();

                while (state != null)
                {
                    path.Push(state);
                    state = state.GetParent();
                }

                while (path.Count > 0)
                {
                    KonumDegismesi(path.Pop().GetState(), path.Count == 0);
                }
            }
            else
            {
                KonumDegismesi(null, true);
            }
        }

        private void Cozulunce(State state, int states)
        {
            int adim = -1;

            while (state != null)
            {
                state = state.GetParent();
                adim++;
            }

            if (OnCozum != null)
            {
                OnCozum(adim, (int)mStopWatch.ElapsedMilliseconds, states);
            }
        }

        #endregion Methods
    }

    internal sealed class MinPriorityQueue<T> where T : IComparable
    {
        #region Fields

        private T[] mArray;
        private int mCount;

        #endregion Fields

        #region Methods

        internal MinPriorityQueue(int capacity)
        {
            mArray = new T[capacity + 1];
            mCount = 0;
        }

        private void Expand(int capacity)
        {
            T[] temp = new T[capacity + 1];
            int i = 0;
            while (++i <= mCount)
            {
                temp[i] = mArray[i];
                mArray[i] = default(T);
            }

            mArray = temp;
        }

        private bool Less(int i, int j)
        {
            return mArray[i].CompareTo(mArray[j]) < 0;
        }

        private void Degis(int i, int j)
        {
            T temp = mArray[j];
            mArray[j] = mArray[i];
            mArray[i] = temp;
        }

        private void Sink(int index)
        {
            int k;
            while (index * 2 <= mCount)
            {
                k = index * 2;

                if (k + 1 <= mCount && Less(k + 1, k))
                {
                    k = k + 1;
                }

                if (!Less(k, index))
                {
                    break;
                }

                Degis(index, k);
                index = k;
            }
        }

        private void Swim(int index)
        {
            int k;

            while (index / 2 > 0)
            {
                k = index / 2;

                if (!Less(index, k))
                {
                    break;
                }

                Degis(index, k);
                index = k;
            }
        }

        internal bool IsEmpty()
        {
            return mCount == 0;
        }

        internal void Enqueue(T item)
        {
            if (mCount == mArray.Length - 1)
            {
                Expand(mArray.Length * 3);
            }

            mArray[++mCount] = item;
            Swim(mCount);
        }

        internal T Dequeue()
        {
            if (!IsEmpty())
            {
                T item = mArray[1];
                mArray[1] = mArray[mCount];
                mArray[mCount--] = default(T);

                Sink(1);

                return item;
            }

            return default(T);
        }

        internal T Find(T item, out int index)
        {
            index = -1;
            if (!IsEmpty())
            {
                int i = 0;

                while (++i <= mCount)
                {
                    if (mArray[i].Equals(item))
                    {
                        index = i;
                        return mArray[i];
                    }
                }
            }

            return default(T);
        }

        internal void Kaldir(int index)
        {
            if (index > 0 && index <= mCount)
            {
                mArray[index] = mArray[mCount];
                mArray[mCount--] = default(T);
                Sink(index);
            }
        }

        #endregion Methods
    }

    internal sealed class LinearKaristir<T>
    {

        private Random mRandom;


        internal LinearKaristir()
        {
            int seed = 37 + 37 * ((int)DateTime.Now.TimeOfDay.TotalSeconds % 37);
            mRandom = new Random(seed);
        }

        internal void Shuffle(T[] array)
        {
            int position;
            for (int i = 0; i < array.Length; i++)
            {
                position = Rastgele(0, i);
                Swap(array, i, position);
            }
        }

        private int Rastgele(int min, int max)
        {
            return mRandom.Next(min, max);
        }

        private void Swap(T[] a, int i, int j)
        {
            T temp = a[i];
            a[i] = a[j];
            a[j] = temp;
        }

    }
}

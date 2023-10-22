using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NCalc;


namespace laba3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch w1 = new Stopwatch();
            Stopwatch w2 = new Stopwatch();

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            for (int i = 0; i < 6; i++)
            {
                w1.Start();
                string[] func = File.ReadAllLines($"data{i}.txt");

                oneThreading(func);

                w1.Stop();
                chart1.Series[0].Points.AddXY(func.Length, w1.ElapsedMilliseconds);

            }
            for (int i = 0; i < 6; i++)
            {
                string[] func = File.ReadAllLines($"data{i}.txt");

                w1.Restart();

                Task task1 = Task.Run(() => manyThreading(func));

                w1.Stop();

                task1.Wait();

                chart1.Series[1].Points.AddXY(func.Length, w1.ElapsedMilliseconds);

            }


        }


        public void oneThreading(string[] func)
        {

            for (int i = 0; i < func.Length; i++)
                    ConjugateGradientMethod(func[i]);

        }

        public void manyThreading(string[] func)
        {

            Task[] tasks = new Task[func.Length-1];


            for(int i = 0; i < func.Length-1; i++)
            {

                tasks[i] = Task.Run(() => { ConjugateGradientMethod(func[i]); });
            }


            Task.WaitAll(tasks);
        }

        //static string randFunc()
        //{
        //    Random rand = new Random();
        //    return rand.Next(-100, 100).ToString() + " * x1 * x1" +
        //      "+" + rand.Next(-100, 100).ToString() + " * x1 * x2" +
        //      "+" + rand.Next(-100, 100).ToString() + " * x2 * x2" +
        //      "+" + rand.Next(-100, 100).ToString() + " * x1" +
        //      "+" + rand.Next(-100, 100).ToString() + " * x2" +
        //      "+" + rand.Next(-100, 100).ToString();
        //}


        static void ConjugateGradientMethod(string function)
        {
            // Задаем начальное значение итераций
            int iterations = 0;

            // Задаем начальные значения для переменных
            double x1 = 0, x2 = 0;

            // Задаем начальные значения для градиента и предыдущего градиента
            double gradX1 = 0, gradX2 = 0, prevGradX1 = 0, prevGradX2 = 0;

            // Задаем начальные значения для направлений поиска и предыдущих направлений поиска
            double dirX1 = 0, dirX2 = 0, prevDirX1 = 0, prevDirX2 = 0;

            // Задаем значение точности, при которой считаем результат достигнутым
            double epsilon = 0.01;

            Stopwatch w = new Stopwatch();

            w.Start();



            // Пока не достигли нужной точности
            while (true)
            {

                // Подставляем текущие значения переменных в функцию
                double result = evaluateFunction(function, x1, x2);

                // Рассчитываем градиент функции для текущих значений переменных
                gradX1 = evaluateGradient(function, "x1", x1, x2);
                gradX2 = evaluateGradient(function, "x2", x1, x2);

                double t = Math.Sqrt(gradX1 * gradX1 + gradX2 * gradX2);

                // Если достигнута нужная точность
                if (t < epsilon || iterations == 20)
                {
                    //Console.WriteLine("Результат: f({0}, {1}) = {2}", x1, x2, result);
                    break;
                }

                // Если достигнута первая итерация
                if (iterations == 0)
                {
                    // Назначаем направлениями градиента
                    dirX1 = -gradX1;
                    dirX2 = -gradX2;
                }
                else
                {
                    // Рассчитываем коэффициент бета
                    double beta = (gradX1 * (gradX1 - prevGradX1) + gradX2 * (gradX2 - prevGradX2)) /
                        (prevGradX1 * prevGradX1 + prevGradX2 * prevGradX2);

                    // Рассчитываем новые направления поиска
                    dirX1 = -gradX1 + beta * prevDirX1;
                    dirX2 = -gradX2 + beta * prevDirX2;
                }

                // Рассчитываем шаг градиентного спуска
                double step = goldenSectionSearch(function, x1, x2, dirX1, dirX2);

                // Обновляем значения переменных
                double prevX1 = x1, prevX2 = x2;
                x1 = prevX1 + step * dirX1;
                x2 = prevX2 + step * dirX2;

                // Обновляем значения градиента и направлений поиска
                prevGradX1 = gradX1;
                prevGradX2 = gradX2;
                prevDirX1 = dirX1;
                prevDirX2 = dirX2;

                iterations++;
            }
        }
        static double evaluateFunction(string function, double x1, double x2)
        {
            var expr = new Expression(function);
            expr.Parameters["x1"] = x1;
            expr.Parameters["x2"] = x2;

            double result = (double)expr.Evaluate();
            return result;


        }

        static double evaluateGradient(string function, string variable, double x1, double x2)
        {
            // Вычисляем градиент функции по указанной переменной
            if (variable == "x1")
            {
                return (evaluateFunction(function, x1 + 0.00001, x2) - evaluateFunction(function, x1, x2)) / 0.00001;
            }
            else if (variable == "x2")
            {
                return (evaluateFunction(function, x1, x2 + 0.00001) - evaluateFunction(function, x1, x2)) / 0.00001;
            }
            else
            {
                throw new ArgumentException("Неправильная переменная");
            }
        }

        static double goldenSectionSearch(string function, double x1, double x2, double dirX1, double dirX2)
        {
            double a = 0, b = 0.01; // Начальные границы интервала поиска
            double epsilon = 0.00001; // Точность поиска

            double c = b - (b - a) / ((1 + Math.Sqrt(5)) / 2); // Рассчитываем внутреннюю точку
            double d = a + (b - a) / ((1 + Math.Sqrt(5)) / 2); // Рассчитываем вторую внутреннюю точку

            // Пока не достигли нужной точности
            while (Math.Abs(c - d) > epsilon)
            {
                double fc = evaluateFunction(function, x1 + c * dirX1, x2 + c * dirX2);
                double fd = evaluateFunction(function, x1 + d * dirX1, x2 + d * dirX2);

                if (fc < fd)
                {
                    b = d;
                }
                else
                {
                    a = c;
                }

                c = b - (b - a) / ((1 + Math.Sqrt(5)) / 2);
                d = a + (b - a) / ((1 + Math.Sqrt(5)) / 2);
            }

            // Возвращаем середину найденного интервала как шаг градиентного спуска
            return (a + b) / 2;
        }

    }
}

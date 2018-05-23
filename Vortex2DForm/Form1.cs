using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace Vortex2DForm
{
    public partial class Form1 : Form
    {
        const double EPS = 0.01;
        const double PI = 3.1415926;
        const int numTracer = 20000;
        const int numDisp = 5000;
        const int T = 200;
        Random rand = new Random();
        double[,] aposx = new double[numTracer, T];
        double[,] aposy = new double[numTracer, T];

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        

        //在[a,b]范围内产生随机数
        double frand(double a, double b)
        {
            double r = rand.NextDouble();
            return (b - a) * r + a;
        }

        //计算每个涡造成的速度u
        double calcUBySingVor(double x, double y, vortex vortex)
        {
            double r_ij2 = (x - vortex.x) * (x - vortex.x) + (y - vortex.y) * (y - vortex.y);
            return vortex.vort * (vortex.y - y) / (r_ij2 * PI * 2) * (1.0 - Math.Exp(-r_ij2 / (EPS * EPS)));
        }

        //计算每个涡造成的速度v
        double calcVBySingVor(double x, double y, vortex vortex)
        {
            double r_ij2 = (x - vortex.x) * (x - vortex.x) + (y - vortex.y) * (y - vortex.y);
            return vortex.vort * (x - vortex.x) / (r_ij2 * PI * 2) * (1.0 - Math.Exp(-r_ij2 / (EPS * EPS)));
        }

        //计算所有涡造成的速度u
        double calcUByAllVort(double x, double y, List<vortex> vortexies)
        {
            double u = 0;
            foreach (vortex vortex in vortexies)
            {
                u += calcUBySingVor(x, y, vortex);
            }

            return u;
        }

        //计算所有涡造成的速度v
        double calcVByAllVort(double x, double y, List<vortex> vortexies)
        {
            double v = 0;
            foreach (vortex vortex in vortexies)
            {
                v += calcVBySingVor(x, y, vortex);
            }

            return v;
        }

        //三阶龙格库塔
        void rk3(ref double x, ref double y, List<vortex> vortexies, double dt)
        {
            double x0 = x;
            double y0 = y;
            double u0 = calcUByAllVort(x0, y0, vortexies);
            double v0 = calcVByAllVort(x0, y0, vortexies);

            double x1 = x0 + 0.5 * dt * u0;
            double y1 = y0 + 0.5 * dt * v0;
            double u1 = calcUByAllVort(x1, y1, vortexies);
            double v1 = calcVByAllVort(x1, y1, vortexies);

            double x2 = x0 + 0.75 * dt * u1;
            double y2 = y0 + 0.75 * dt * v1;
            double u2 = calcUByAllVort(x2, y2, vortexies);
            double v2 = calcVByAllVort(x2, y2, vortexies);

            x += (0.222222222222 * u0 + 0.333333333333 * u1 + 0.444444444444 * u2) * dt;
            y += (0.222222222222 * v0 + 0.333333333333 * v1 + 0.444444444444 * v2) * dt;

        }

        ////一阶欧拉
        //void euler1(ref double x,ref double y,double dt)
        //{
        //    double u = vecU(x, y);
        //    double v = vecV(x, y);

        //    x += u * dt;
        //    y += v * dt;
        //}

        ////二阶欧拉
        //void euler2(ref double x,ref double y,double dt)
        //{
        //    double x0 = x;
        //    double y0 = y;
        //    double u0 = vecU(x0, y0);
        //    double v0 = vecV(x0, y0);

        //    double x1 = x0 + 0.5 * u0 * dt;
        //    double y1 = y0 + 0.5 * v0 * dt;
        //    double u1 = vecU(x1, y1);
        //    double v1 = vecV(x1, y1);

        //    x += 0.5 * (u0 + u1) * dt;
        //    y += 0.5 * (v0 + v1) * dt;
        //}

        ////三阶
        //void euler3(ref double x, ref double y, double dt)
        //{
        //    double x0 = x;
        //    double y0 = y;
        //    double u0 = vecU(x0, y0);
        //    double v0 = vecV(x0, y0);

        //    double x1 = x0 + 0.5 * u0 * dt;
        //    double y1 = y0 + 0.5 * v0 * dt;
        //    double u1 = vecU(x1, y1);
        //    double v1 = vecV(x1, y1);
        //    double x2 = x0 + 0.75 * u1 * dt;
        //    double y2 = y0 + 0.75 * v1 * dt;
        //    double u2 = vecU(x2, y2);
        //    double v2 = vecV(x2, y2);

        //    x += (u0 + 4 * u1 + u2) * dt / 6;
        //    y += (v0 + 4 * v1 + v2) * dt / 6;
        //}
        /// 


        //写出计算结果
        void writeFile(double[] posx, double[] posy, int num, int frame)
        {
            double[] data = new double[num * 4];

            for (int i = 0; i < num; i++)
            {
                data[i * 4 + 0] = posx[i];
                data[i * 4 + 1] = posy[i];
                data[i * 4 + 2] = 0;
                data[i * 4 + 3] = 1;
            }

            using (StreamWriter sw = new StreamWriter(frame.ToString("D3") + ".bin"))
            {
                for (int i = 0; i < num; i++)
                {
                    sw.WriteLine("{0} {1} {2} {3}", data[i * 4 + 0], data[i * 4 + 1], data[i * 4 + 2], data[i * 4 + 3]);
                }

            }
        }

        //开始模拟
        void startSimulate()
        {

            //初始条件,两对涡,坐标(0,1) (0,-1),(0,0.3) (0,-0.3),涡量(1,-1)
            List<vortex> vortexies = new List<vortex>()
            {
                new vortex(0.0,1.0,1.0),
                new vortex(0.0,-1.0,-1.0),
                new vortex(0.0,0.3,1.0),
                new vortex(0.0,-0.3,-1.0),
            };

            //初始化粒子群
            double[] posx = new double[numTracer];
            double[] posy = new double[numTracer];

            for (int i = 0; i < numTracer; i++)
            {
                double x = frand(-0.5, 0.5);
                double y = frand(-1.5, 1.5);
                posx[i] = x;
                posy[i] = y;
            }

            //开始模拟
            double dt = 0.1;
            for (int t = 0; t < T; t++)
            {
                writeFile(posx, posy, numTracer, t);
                //paintParticals(posx, posy);
                //3阶龙格库塔求解粒子位置
                for (int i = 0; i < numTracer; i++)
                {
                    rk3(ref posx[i], ref posy[i], vortexies, dt);
                    //euler3(ref posx[i], ref posy[i], dt);
                }

                //添加用于显示的粒子
                for (int i = 0; i < numDisp; i++)
                {
                    aposx[i, t] = posx[i];
                    aposy[i, t] = posy[i];
                }


                //求解每个涡在其他涡作用下的速度和位移
                List<vortex> vortexiesTemp = new List<vortex>();
                foreach (vortex vortex in vortexies)
                {
                    vortexiesTemp.Add(new vortex(vortex.x, vortex.y, vortex.vort));
                }
                for (int i = 0; i < vortexies.Count; i++)
                {
                    double u = 0;
                    double v = 0;
                    for (int j = 0; j < vortexies.Count; j++)
                    {
                        if (j != i)
                        {
                            u += calcUBySingVor(vortexies[i].x, vortexies[i].y, vortexies[j]);
                            v += calcVBySingVor(vortexies[i].x, vortexies[i].y, vortexies[j]);
                        }
                    }
                    vortexiesTemp[i].x += dt * u;
                    vortexiesTemp[i].y += dt * v;
                }

                for (int i = 0; i < vortexiesTemp.Count; i++)
                {
                    vortexies[i] = vortexiesTemp[i];
                }

                dispStep(t);
                dispProgress(t);
                //this.Invoke(myHandle, t);
                Console.WriteLine(button1.Text);
            }
        }

        private delegate void progressDisp(int t);


        //粒子绘制
        private void paintParticals(double[] posx,double[] posy)
        {
            using (Graphics g = this.CreateGraphics())
            {
                g.FillRectangle(Brushes.White, this.ClientRectangle);
                for (int i = 0; i < posx.Length; i++)
                {
                    g.FillEllipse(Brushes.Red, (float)(posx[i] * 50.0 + 50), (float)(posy[i] * 50.0 + this.ClientSize.Height / 2 - 10), 1, 1);
                }
            }
        }


        //double vecU(double x,double y)
        //{
        //    return Math.Abs(y * y - 1.5*1.5);
        //}

        //double vecV(double x,double y)
        //{
        //    return 0;
        //}




        /// <summary>
        /// 显示计算进度
        /// </summary>
        /// <param name="t"></param>
        private void dispStep(int t)
        {
            //if (button1.InvokeRequired)
            //{
            //    progressDisp progressDisp = new progressDisp(dispStep);
            //    Invoke(progressDisp, new object[] { t });
            //}
            //else
                button1.Text = "step " + t + "/" + T + " is done!";
        }
        private void dispProgress(int t)
        { 
            //if (textBox1.InvokeRequired)
            //{
            //    progressDisp progressDisp = new progressDisp(dispProgress);
            //    Invoke(progressDisp, new object[] { t });
            //}
            //else
            //{
                if (t % (T / 100) == 0 && t > 0)
                {
                    textBox1.Text += "=";
                    double[] posx = new double[numTracer];
                    double[] posy = new double[numTracer];

                    for (int i = 0; i < numDisp; i++)
                    {
                        posx[i] = aposx[i, t];
                        posy[i] = aposy[i, t];
                    }
                    paintParticals(posx, posy);
                }
            if (t >= T - 1)
            {
                button1.Text = "Play Animation";
                thread.Abort();
            }
            //}
            
        }


        int isClicked = 0;      //按钮是否被点击过
        Thread thread = null;   //线程 用于后台计算
        private void button1_Click(object sender, EventArgs e)
        {
            //功能实现:点击按钮开始计算,再次点击暂停计算 方法已过时,需要修改!!!
            if (isClicked == 0 && thread == null)
            {
                thread = new Thread(new ThreadStart(startSimulate));
                isClicked = 1;
                thread.Start();
            }
            else if (isClicked == 0 && thread != null)
            {
                isClicked = 1;
                thread.Resume();
            }
            else if(thread.IsAlive)
            {
                isClicked = 0;
                button1.Text = "Pause!";
                thread.Suspend();
            }
            if (button1.Text == "Play Animation")
            {
                playAnim();
            }
            
            
        }

        private void playAnim()
        {
            for (int i = 0; i < T; i++)
            {
                double[] posx = new double[numTracer];
                double[] posy = new double[numTracer];

                for (int j = 0; j < numTracer; j++)
                {
                    posx[j] = aposx[j, i];
                    posy[j] = aposy[j, i];
                }
                paintParticals(posx, posy);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
            
            Application.Exit();
        }
    }
}

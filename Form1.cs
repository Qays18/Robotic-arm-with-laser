using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;



namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;

        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> BWImg;
        
        private double myXScale, myYScale;
        private int Xpx, Ypx, N;
        private double Xcm, Ycm, Zcm = 130, d1 = 10.0, Px, Py, Pz;

        static SerialPort _serialPort;
        public byte[] Buff = new byte[2];
        public Form1()
        {
            InitializeComponent();

            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";//Set your board COM
            _serialPort.BaudRate = 9600;
            _serialPort.Open();
        }
        
       private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)//very important to handel excption
            {
                try
                {
                    capture = new Capture(); 
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();
             GrayImg = IMG.Convert<Gray, Byte>();
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(54), new Gray(255));

            Xpx = 0;
            Ypx = 0;
            N = 0;

            // Adding a front image
            for (int i = 0; i < BWImg.Width; i++)
            {
                for (int j = 0; j < BWImg.Height; j++)
                {
                    if (BWImg[j, i].Intensity > 128)
                    {
                        N++;
                        Xpx += i;
                        Ypx += j;
                    }
                }
            }

            if (N > 0)// if there is an object
            {
                // the center point of the forground of object in pixels
                Xpx = Xpx / N;
                Ypx = Ypx / N;
                myXScale = 16.0 / 177.0/ BWImg.Width;
                myYScale = 177.0 / BWImg.Height;
                // the center point of the forground of object in centimeters
              
                Xcm = (Xpx - 320 ) * myXScale;
                Ycm = (240 - Ypx ) * myYScale;

                textBox1.Text = Xcm.ToString();
                textBox2.Text = Ycm.ToString();
                textBox3.Text = N.ToString();
                Px = Zcm;
                Py = -Xcm;
                Pz = Ycm + d1;

                // Inverse K. model
                double Th1 = Math.Atan(Py / Px);
                double Th2 = Math.Atan((Math.Sin(Th1)*Pz)/Py) ;
                
               Th1 = Th1 * (180 / Math.PI) + 90;
               
               Th2 = Th2 * (180 / Math.PI) + 90;
               

                textBox4.Text = Th1.ToString();
                textBox5.Text = Th2.ToString();
                
               

                Buff[0] = (byte)Th1; 
                Buff[1] = (byte)Th2; 
                _serialPort.Write(Buff, 0, 2);

            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = N.ToString();

            }

            try
            {

                imageBox1.Image = IMG;
                imageBox2.Image = GrayImg;
                imageBox3.Image = BWImg;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        private void button1_Click(object sender, EventArgs e)
        {
            //Application.Idle += processFrame;
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {
            //Application.Idle -= processFrame;
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" + ".jpg");
        }
		void Timer1Tick(object sender, EventArgs e)
		{
	       processFrame(sender, e);
		}
		void Button1Click(object sender, EventArgs e)
		{
	//Application.Idle += processFrame;
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
		}
		void Button2Click(object sender, EventArgs e)
		{
	 //Application.Idle -= processFrame;
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
		}
		void Button3Click(object sender, EventArgs e)
		{
	    IMG.Save("Image" + ".jpg");
		}
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

    }
}

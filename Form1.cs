using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Collections.Concurrent;

namespace MyClassroom
{
    public partial class Form1 : Form
    {
        //四间教室 每间教室13个指标
        //0-区域一风扇 1-区域二风扇 2-区域三风扇
        //3-区域一照明 4-区域二照明 5-区域三照明
        //6-教室使用状态
        //7-空调开关状态
        //8-区域一人体红外 9-区域二人体红外 10-区域三人体红外
        //11-温度 12-湿度
        byte[][] classArray = new byte[4][];

        GroupBox[] group; //四个教室
        Label[] classState; //教室空闲or上课
        Label[] temp;
        Label[] water;
        Label[] airc;
        Label[][] fan = new Label[4][];
        Label[][] light = new Label[4][];
        Label[][] sb = new Label[4][];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //初始化数据
            classArray[0] = new byte[13] {   0 ,  0  , 0 ,  0  , 0   ,0  , 0  , 0 ,  0  , 0,   0,   25 , 49 };
            classArray[1] = new byte[13] { 0 ,0 ,  0  , 1 ,  1 ,  1 ,  1  , 0,   1 ,  1 ,  1  , 24 ,  50 };
            classArray[2] = new byte[13] { 0   , 0  , 0  , 1 ,  1  , 1 ,  2  , 0  , 1 ,  1 ,  1 ,  25 , 42 };
            classArray[3] = new byte[13] { 0  ,  0  , 0 ,  1,   1   ,1   ,2  , 0 ,  1  , 1,   1 ,  25 , 51 };

            updataData_2(classArray);//更新数据
            serialPort1.ReceivedBytesThreshold = 56;
            updataTimer();

            //设置下方下拉框
            int[] temp_set = new int[4] {15,20,25,30 };
            String[] time_set = new string[24];
            for (int i = 0; i < 24; i++)
            {
                time_set[i] = i + ":00";
            }

            for (int i = 0; i < 4; i++)
            {
                comboBox1.Items.Add(temp_set[i]);
            }

            for (int i = 0; i < 24; i++)
            {
                comboBox2.Items.Add(time_set[i]);
                comboBox3.Items.Add(time_set[i]);
                comboBox4.Items.Add(time_set[i]);
                comboBox5.Items.Add(time_set[i]);
            }

        }

        public void updataData_2(byte[][] classArray)
        {

            //四间教室
            group = new GroupBox[4] { groupBox0, groupBox1, groupBox2, groupBox3 };
            //教室状态Label
            classState = new Label[4] { classState0, classState1, classState2, classState3 };
            //温度
            temp = new Label[4] { temp0, temp1, temp2, temp3 };
            //湿度
            water = new Label[] { water0, water1, water2, water3 };
            //空调状态
            airc = new Label[] { airc0, airc1, airc2, airc3 };
            //三个区域的值
            //风扇
            fan[0] = new Label[] { fan00, fan01, fan02 };
            fan[1] = new Label[] { fan10, fan11, fan12 };
            fan[2] = new Label[] { fan20, fan21, fan22 };
            fan[3] = new Label[] { fan30, fan31, fan32 };
            //灯
            light[0] = new Label[] { light00, light01, light02 };
            light[1] = new Label[] { light10, light11, light12 };
            light[2] = new Label[] { light20, light21, light22 };
            light[3] = new Label[] { light30, light31, light32 };
            //人
            sb[0] = new Label[] { sb00, sb01, sb02 };
            sb[1] = new Label[] { sb10, sb11, sb12 };
            sb[2] = new Label[] { sb20, sb21, sb22 };
            sb[3] = new Label[] { sb30, sb31, sb32 };

            //0-区域一风扇 1-区域二风扇 2-区域三风扇
            //3-区域一照明 4-区域二照明 5-区域三照明
            //6-教室使用状态
            //7-空调开关状态
            //8-区域一人体红外 9-区域二人体红外 10-区域三人体红外
            //11-温度 12-湿度

            //根据数据改变界面
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    //改变背景颜色和状态显示
                    if (classArray[i][6] == 2)
                    {
                        classState[i].Text = "上课中";
                        group[i].BackColor = Color.Red;
                    }
                    else if (classArray[i][6] == 1)
                    {
                        classState[i].Text = "无课人多";
                        group[i].BackColor = Color.LightYellow;
                    }
                    else
                    {
                        classState[i].Text = "空闲中";
                        group[i].BackColor = Color.LightBlue;
                    }
                    //改变温度和湿度
                    temp[i].Text = classArray[i][11].ToString();
                    water[i].Text = classArray[i][12].ToString();
                    //空调
                    if (classArray[i][7] == 0)
                    {
                        airc[i].Text = "关";
                    }
                    else
                    {
                        airc[i].Text = "开";
                    }

                }
                //改变三个区域的值
                for (int k = 0; k < 3; k++)
                {
                    if (classArray[i][k] == 0) { fan[i][k].Text = "风扇:关"; }
                    else { fan[i][k].Text = "风扇:开"; }
                    if (classArray[i][k + 3] == 0) { light[i][k].Text = "照明:关"; }
                    else { light[i][k].Text = "照明:开"; }
                    if (classArray[i][k + 8] == 0) { sb[i][k].Text = "无人"; }
                    else { sb[i][k].Text = "有人"; }
                }
            }
            //根据当前数据设置推荐教室
            command.Text = getCommandClassroom(classArray);

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void updataData(byte[][] classArray)
        {
            
            //四间教室
            group = new GroupBox[4] { groupBox0, groupBox1, groupBox2, groupBox3 };
            //教室状态Label
            classState = new Label[4] { classState0, classState1, classState2, classState3 };
            //温度
            temp = new Label[4] { temp0, temp1, temp2, temp3 };
            //湿度
            water = new Label[] { water0, water1, water2, water3 };
            //空调状态
            airc = new Label[] { airc0, airc1, airc2, airc3 };
            //三个区域的值
            //风扇
            fan[0] = new Label[] { fan00, fan01, fan02 };
            fan[1] = new Label[] { fan10, fan11, fan12 };
            fan[2] = new Label[] { fan20, fan21, fan22 };
            fan[3] = new Label[] { fan30, fan31, fan32 };
            //灯
            light[0] = new Label[] { light00, light01, light02 };
            light[1] = new Label[] { light10, light11, light12 };
            light[2] = new Label[] { light20, light21, light22 };
            light[3] = new Label[] { light30, light31, light32 };
            //人
            sb[0] = new Label[] { sb00, sb01, sb02 };
            sb[1] = new Label[] { sb10, sb11, sb12 };
            sb[2] = new Label[] { sb20, sb21, sb22 };
            sb[3] = new Label[] { sb30, sb31, sb32 };

            //0-区域一风扇 1-区域二风扇 2-区域三风扇
            //3-区域一照明 4-区域二照明 5-区域三照明
            //6-教室使用状态
            //7-空调开关状态
            //8-区域一人体红外 9-区域二人体红外 10-区域三人体红外
            //11-温度 12-湿度

            //根据数据改变界面
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    //改变背景颜色和状态显示
                    if(classArray[i][6] == 2)
                    {
                        classState[i].Text = "上课中";
                        group[i].BackColor = Color.Red;
                    }
                    else if(classArray[i][6] == 1)
                    {
                        classState[i].Text = "无课人多";
                        group[i].BackColor = Color.LightYellow;
                    }else
                    {
                        classState[i].Text = "空闲中";
                        group[i].BackColor = Color.LightBlue;
                    }
                    //改变温度和湿度
                    temp[i].Text = classArray[i][11].ToString();
                    water[i].Text = classArray[i][12].ToString();
                    //空调
                    if(classArray[i][7] == 0)
                    {
                        airc[i].Text = "关";
                    }
                    else
                    {
                        airc[i].Text = "开";
                    }
                    
                }
                //改变三个区域的值
                for (int k = 0; k < 3; k++)
                {
                    if (classArray[i][k] == 0) { fan[i][k].Text = "风扇:关"; }
                    else { fan[i][k].Text = "风扇:开"; }
                    if (classArray[i][k + 3] == 0) { light[i][k].Text = "照明:关"; }
                    else { light[i][k].Text = "照明:开"; }
                    if (classArray[i][k + 8] == 0) { sb[i][k].Text = "无人"; }
                    else { sb[i][k].Text = "有人"; }
                }
            }
            //根据当前数据设置推荐教室
            //command.Text = getCommandClassroom(classArray);

        }

        
	    class cla
        {
            public int num;
            public int dat;
            
        }
        public String getCommandClassroom(byte[][] classArray)
        {
            //等hhn的算法，现在直接返回教室-1

            cla[] myList = new cla[4];
            
            int[] rList = new int[4];

            for (int i = 0; i < 4; i++)
            {
                myList[i] = new cla();
                myList[i].num = i;
            }

            myList[0].dat = classArray[0][0] + classArray[0][1] + classArray[0][2] + classArray[0][3] + classArray[0][4] + classArray[0][5] + classArray[0][8] + classArray[0][9] + classArray[0][10] + classArray[0][11];
            myList[1].dat = classArray[1][0] + classArray[1][1] + classArray[1][2] + classArray[1][3] + classArray[1][4] + classArray[1][5] + classArray[1][8] + classArray[1][9] + classArray[1][10] + classArray[1][11];
            myList[2].dat = classArray[2][0] + classArray[2][1] + classArray[2][2] + classArray[2][3] + classArray[2][4] + classArray[2][5] + classArray[2][8] + classArray[2][9] + classArray[2][10] + classArray[2][11];
            myList[3].dat = classArray[3][0] + classArray[3][1] + classArray[3][2] + classArray[3][3] + classArray[3][4] + classArray[3][5] + classArray[3][8] + classArray[3][9] + classArray[3][10] + classArray[3][11];


            int[] rList_b = rList;
            //从小到大排序
            Array.Sort(rList);

            string result = "";

            for (int i = 0; i < 3; i++) { 
                for (int j = i + 1; j < 4; j++)
                {
                    if (myList[i].dat < myList[j].dat)
                    {
                        int temp1, temp2;
                        temp1 = myList[i].dat;
                        temp2 = myList[i].num;
                        myList[i].dat = myList[j].dat;
                        myList[i].num = myList[j].num;
                        myList[j].dat = temp1;
                        myList[j].num = temp2;
                    }
                }
            }
            if(classArray[myList[1].num][6] == 1)
            {
                result = "教室"+ myList[1].num;
            }
            else
            {
                if (classArray[myList[2].num][6] == 1)
                {
                    result = "教室" + myList[2].num;
                }
                else
                {
                    if (classArray[myList[3].num][6] == 1)
                    {
                        result = "教室" + myList[3].num;
                    }
                    else
                    {
                        if (classArray[myList[0].num][6] == 1)
                        {
                            result = "教室" + myList[0].num;
                        }
                        else
                        {
                            result = "无空闲教室";
                        }
                    }
                }
            }
 
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updataTimer();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //115200
        }

        public void updataTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;//执行间隔时间,单位为毫秒    
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer1_Elapsed);
        }
            
        private void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.Open();
            }
            byte[] order = new byte[6] { 0x3A, 0x00, 0x00, 0x05, 0x3F, 0x23 };
            serialPort1.Write(order, 0, 6);
            
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //serialPort1.ReadExisting();
            //子线程更新UI
            //byte[] data = Convert.FromBase64String(serialPort1.ReadExisting());

            int Readlen = 56;
            byte[] readBuffer = new byte[Readlen];
            serialPort1.Read(readBuffer, 0, Readlen);
            
            String str = "";
            for (int i = 0; i < readBuffer.Length; i++)
            {
                str += readBuffer[i]+" ";
            }

            int n = 4;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    if (n <= readBuffer.Length)
                    {
                        classArray[i][j] = readBuffer[n];
                        n++;
                    }
                    else
                    {
                        classArray[i][j] = 0;
                    }
                }
            }

            String str2 = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    str2 += classArray[i][j].ToString() + " ";
                }
            }
            str2 += "长度：" + n.ToString();   

            SerialPort sp = (SerialPort)sender;

            this.Invoke(new EventHandler(delegate
            {
                //获取传过来的值
                //textBox1.Text = serialPort1.ReadExisting().GetType().ToString();
                //textBox1.Text = serialPort1.ReadExisting().ToString();

                //textBox1.Text = readBuffer.Length.ToString();
                //textBox1.AppendText(str2);
                //textBox1.Text = str + " 长度："+ Readlen;
                updataData(classArray);
            }));
            serialPort1.DiscardInBuffer();
            serialPort1.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void g_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            int temp = (int)comboBox1.SelectedItem;
            byte byte_temp = Convert.ToByte(temp);
            //MessageBox.Show(temp+"");
            byte check = (byte)(0x3A ^ 0x00 ^ 0x0A ^ byte_temp);
            //MessageBox.Show(check+"");
            byte[] order = new byte[6] { 0x3A,0x00, 0x0A, byte_temp, check, 0x23 };
            //MessageBox.Show(order.ToString());
            if (!serialPort1.IsOpen)
            {
                serialPort1.Open();
            }
            serialPort1.Write(order, 0, 6);
        }


        
        private void button2_Click_1(object sender, EventArgs e)
        {

            if (comboBox2.SelectedIndex < comboBox3.SelectedIndex)
            {
                MessageBox.Show("请选择正确的时间");
            }
            else
            {
                String str = "";
                int time_b = (int)comboBox2.SelectedIndex;
                int time_e = (int)comboBox3.SelectedIndex;

                byte byte_time_b = Convert.ToByte(time_b);
                byte byte_time_e = Convert.ToByte(time_e);
                byte check = (byte)(0x3A ^ 0x00 ^ 0x0B ^ byte_time_b ^ byte_time_e);
                str += time_b + " " + time_e + " " + byte_time_b + " " + byte_time_e + " " + check;
                //MessageBox.Show(str + "");
                byte[] order_1 = new byte[7] { 0x3A, 0x00, 0x0B, byte_time_b, byte_time_e, check, 0x23 };

                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                }
                serialPort1.Write(order_1, 0, 7);

            }
        }
        public void ThreadMethod(object byte_arr)
        {
           
            
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex < comboBox5.SelectedIndex)
            {
                MessageBox.Show("请选择正确的时间");
            }
            else
            {
                int time_b = (int)comboBox4.SelectedIndex + 1;
                int time_e = (int)comboBox5.SelectedIndex + 1;
                byte byte_time_b = Convert.ToByte(time_b);
                byte byte_time_e = Convert.ToByte(time_e);
                byte check = (byte)(0x3A ^ 0x00 ^ 0x0C ^ byte_time_b ^ byte_time_e);
                //MessageBox.Show(temp+"");
                if (!serialPort1.IsOpen)
                {
                    serialPort1.Open();
                }
                byte[] order = new byte[7] { 0x3A, 0x00, 0x0C, byte_time_b, byte_time_e, check, 0x23 };
                //serialPort1.Write(order, 0, 7);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            updataTimer();
        }
    }
}

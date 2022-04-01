﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.XINJE;
using HslCommunication;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
	public partial class FormXinJEInternalTcp : HslFormContent
	{
		public FormXinJEInternalTcp( )
		{
			InitializeComponent( );
			xinJE = new XinJEInternalNet( );
		}


		private XinJEInternalNet xinJE = null;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "XINJE Read PLC Demo";

				label27.Text = "Ip:";
				label26.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				label21.Text = "Address:";

				label11.Text = "Address:";
				label12.Text = "length:";
				button25.Text = "Bulk Read";
				label13.Text = "Results:";
				label16.Text = "Message:";
				label14.Text = "Results:";
				button26.Text = "Read";

				groupBox3.Text = "Bulk Read test";
				groupBox4.Text = "Message reading test, hex string needs to be filled in";
				groupBox5.Text = "Special function test";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		
		#region Connect And Close

		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox2.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			xinJE?.ConnectClose( );
			xinJE = new XinJEInternalNet( );
			xinJE.IpAddress = textBox1.Text;
			xinJE.Port = port;

			try
			{
				xinJE.Station = byte.Parse( textBox15.Text );
				OperateResult connect = xinJE.ConnectServer( );
				if (connect.IsSuccess)
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );

					button2.Enabled = true;
					button1.Enabled = false;
					panel2.Enabled = true;

					userControlReadWriteOp1.SetReadWriteNet( xinJE, "D100", true );
				}
				else
				{
					MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed );
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			xinJE.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}



		#endregion

		#region 批量读取测试

		private void button25_Click( object sender, EventArgs e )
		{
			DemoUtils.BulkReadRenderResult( xinJE, textBox6, textBox9, textBox10 );
		}



		#endregion

		#region 报文读取测试


		private void button26_Click( object sender, EventArgs e )
		{
			OperateResult<byte[]> read = xinJE.ReadFromCoreServer( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) );
			if (read.IsSuccess)
			{
				textBox11.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read.Content );
			}
			else
			{
				MessageBox.Show( "Read Failed：" + read.ToMessageShowString( ) );
			}
		}


		#endregion
		
		#region 测试使用

		private void test3( )
		{
			short d100_short = xinJE.ReadInt16( "D100" ).Content;
			ushort d100_ushort = xinJE.ReadUInt16( "D100" ).Content;
			int d100_int = xinJE.ReadInt32( "D100" ).Content;
			uint d100_uint = xinJE.ReadUInt32( "D100" ).Content;
			long d100_long = xinJE.ReadInt64( "D100" ).Content;
			ulong d100_ulong = xinJE.ReadUInt64( "D100" ).Content;
			float d100_float = xinJE.ReadFloat( "D100" ).Content;
			double d100_double = xinJE.ReadDouble( "D100" ).Content;
			// need to specify the text length
			string d100_string = xinJE.ReadString( "D100", 10 ).Content;
		}
		private void test4( )
		{
			xinJE.Write( "D100", (short)5 );
			xinJE.Write( "D100", (ushort)5 );
			xinJE.Write( "D100", 5 );
			xinJE.Write( "D100", (uint)5 );
			xinJE.Write( "D100", (long)5 );
			xinJE.Write( "D100", (ulong)5 );
			xinJE.Write( "D100", 5f );
			xinJE.Write( "D100", 5d );
			// length should Multiples of 2 
			xinJE.Write( "D100", "12345678" );
		}


		private void test5( )
		{
			OperateResult<byte[]> read = xinJE.Read( "D100", 10 );
			if(read.IsSuccess)
			{
				int count = xinJE.ByteTransform.TransInt32( read.Content, 0 );
				float temp = xinJE.ByteTransform.TransSingle( read.Content, 4 );
				short name1 = xinJE.ByteTransform.TransInt16( read.Content, 8 );
				string barcode = Encoding.ASCII.GetString( read.Content, 10, 10 );
			}
		}

		private void test6( )
		{
			OperateResult<UserType> read = xinJE.ReadCustomer<UserType>( "D100" );
			if (read.IsSuccess)
			{
				UserType value = read.Content;
			}
			// write value
			xinJE.WriteCustomer( "D100", new UserType( ) );

			xinJE.LogNet = new HslCommunication.LogNet.LogNetSingle( Application.StartupPath + "\\Logs.txt" );

		}


		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox1.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;
			textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}

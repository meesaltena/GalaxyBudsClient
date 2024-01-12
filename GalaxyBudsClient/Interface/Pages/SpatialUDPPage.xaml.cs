using System.Net.Sockets;
using System.Net;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using System;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class SpatialUDPPage : AbstractPage
	{
		public override Pages PageType => Pages.SpatialUDPPage;

		private readonly SpatialSensorManager _spatialSensorManager = new SpatialSensorManager();
		private readonly TextBlock _details;

		private UdpClient _udpClient;
        private IPEndPoint _ep;

		public SpatialUDPPage()
		{   
			AvaloniaXamlLoader.Load(this);
			
			_details = this.FindControl<TextBlock>("SpatialUDPDetails");
			
			try
            {
                _udpClient = new UdpClient();
                _ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50327);
            }
            catch (Exception e)
            {
                throw e;
            }

            _spatialSensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
        }

		private async void OnNewQuaternionReceived(object? sender, Quaternion e)
		{
			var rpy = e.ToRollPitchYaw();
			//var yaw = rpy[0].Remap(-1.5f, 1.5f, -180, 180);
			var yaw = rpy[0].Remap(0, 3, 0, 360);
			var pitch = rpy[1].Remap(-3, 3, 0, 360);
			var roll = rpy[2].Remap(-3, 3, 0, 360);

            byte[] bytesToSend = new byte[48];

			//								 x,y,z, yaw, pitch, roll
			double[] values = new double[] { 0,0,0, yaw, pitch, roll};
            Buffer.BlockCopy(values, 0, bytesToSend, 0, bytesToSend.Length);

            await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, _ep);

			//_details.Text = $"{Loc.Resolve("spatial_udp_dump_quaternion")}\n" +
			//				$"X={e.X}\nY={e.Y}\nZ={e.Z}\nW={e.W}\n\n" +
			//				$"{Loc.Resolve("spatial_udp_dump_rpy")}\n" +
			//				$"Roll={roll}\nPitch={pitch}\nYaw={yaw}\n"; ;
		}

        public override void OnPageShown()
		{
			_spatialSensorManager.Attach();
			
			_details.Text = Loc.Resolve("system_waiting_for_device");
		}
		
		public override void OnPageHidden()
		{
			_spatialSensorManager.Detach();
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}
	}
}

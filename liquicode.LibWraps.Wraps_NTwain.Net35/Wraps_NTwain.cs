

using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

//using NTwain;
//using NTwain.Data;


namespace liquicode.LibWraps
{
	public partial class Wraps_NTwain
	{


		//=====================================================================
		private NTwain.TwainSession _TwainSession;
		private bool _StopCapture = false;


		//=====================================================================
		//public NTwain.DataSource CurrentDataSource = null;
		//public List<NTwain.DataSource> DataSourceList = null;

		public NTwain.Data.SupportedSize CurrentPageSize = NTwain.Data.SupportedSize.USLetter;
		public List<NTwain.Data.SupportedSize> PageSizeList = null;

		public NTwain.Data.TWFix32 CurrentDpi = 300;
		public List<NTwain.Data.TWFix32> DpiList = null;

		public NTwain.Data.PixelType CurrentDepth = NTwain.Data.PixelType.RGB;
		public List<NTwain.Data.PixelType> DepthList = null;



		//=====================================================================
		public void LogMessage( string Message )
		{
			string header = "=== " + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " === ";
			NTwain.PlatformInfo.Current.Log.Info( header + Message );
			this.Fire_LogMessage( new LogMessage_EventArgs( header + Message, null ) );
			return;
		}


		//=====================================================================
		public void SetupTwain()
		{
			var appId = NTwain.Data.TWIdentity.CreateFromAssembly( NTwain.Data.DataGroups.Image, Assembly.GetEntryAssembly() );
			this._TwainSession = new NTwain.TwainSession( appId );
			int _last_state = this._TwainSession.State;

			//------------------------------------------
			this._TwainSession.StateChanged += ( s, e ) =>
			{
				string message = string.Format( "State changed from {0} ({1}) to {2} ({3}).", _last_state, Text4TwainState( _last_state ), this._TwainSession.State, Text4TwainState( this._TwainSession.State ) );
				this.LogMessage( message );
				if( (_last_state < 6) && (this._TwainSession.State == 6) )
				{
					// Start of pass.
				}
				if( (_last_state == 7) && (this._TwainSession.State == 6) )
				{
					// End of page.
				}
				if( (_last_state == 6) && (this._TwainSession.State == 5) )
				{
					// End of document.
				}
				_last_state = this._TwainSession.State;
			};

			//------------------------------------------
			this._TwainSession.TransferReady += ( s, e ) =>
			{
				this.LogMessage( "Transfer ready event." );
				e.CancelAll = _StopCapture;
			};

			//------------------------------------------
			this._TwainSession.DataTransferred += ( s, e ) =>
			{
				this.LogMessage( "Transferred data event." );


				// handle image data
				Image new_image = null;
				if( e.NativeData != IntPtr.Zero )
				{
					// Retrieve data from a stream.
					var stream = e.GetNativeImageStream();
					if( stream != null )
					{
						new_image = Image.FromStream( stream );
					}
				}
				else if( !string.IsNullOrEmpty( e.FileDataPath ) )
				{
					// Retrieve data from a file.
					//new_image = new Bitmap( e.FileDataPath );
					new_image = Image.FromFile( e.FileDataPath );
				}

				if( new_image != null )
				{
					// Report image information.
					var infos = e.GetExtImageInfo( NTwain.Data.ExtendedImageInfo.Camera ).Where( it => it.ReturnCode == NTwain.Data.ReturnCode.Success );
					foreach( NTwain.Data.TWInfo it in infos )
					{
						var values = it.ReadValues();
						this.LogMessage( string.Format( "Image Info: {0} = {1}", it.InfoID, values.FirstOrDefault() ) );
						break;
					}

					this.Fire_PageScanned( new PageScanned_EventArgs( new_image ) );
				}
			};

			//------------------------------------------
			this._TwainSession.SourceDisabled += ( s, e ) =>
			{
				this.LogMessage( "Source disabled event." );
				this.Fire_SourceDisabled( new SourceDisabled_EventArgs() );
			};

			//------------------------------------------
			this._TwainSession.TransferError += ( s, e ) =>
			{
				this.LogMessage( "Got xfer error." );
				this.Fire_TransferError( new TransferError_EventArgs( e.Exception ) );
			};

			//------------------------------------------
			// either set sync context and don't worry about threads during events,
			// or don't and use control.invoke during the events yourself
			this.LogMessage( "Setup thread." );
			this._TwainSession.SynchronizationContext = SynchronizationContext.Current;
			if( this._TwainSession.State < 3 )
			{
				// use this for internal msg loop
				this._TwainSession.Open();
				// use this to hook into current app loop
				//this._twain.Open(new WindowsFormsMessageLoopHook(this.Handle));
			}

			return;
		}


		//=====================================================================
		public void CleanupTwain()
		{
			if( this._TwainSession.State == 4 )
			{
				this._TwainSession.CurrentSource.Close();
			}
			if( this._TwainSession.State == 3 )
			{
				this._TwainSession.Close();
			}

			if( this._TwainSession.State > 2 )
			{
				// normal close down didn't work, do hard kill
				this._TwainSession.ForceStepDown( 2 );
			}
		}


		//=====================================================================
		public NTwain.DataSource CurrentDataSource
		{
			get
			{
				return this._TwainSession.CurrentSource;
			}
		}


		//=====================================================================
		public List<NTwain.DataSource> DataSourceList
		{
			get
			{
				List<NTwain.DataSource> datasources = new List<NTwain.DataSource>();
				foreach( NTwain.DataSource datasource in this._TwainSession )
				{
					datasources.Add( datasource );
				}
				return datasources;
			}
		}


		//=====================================================================
		public bool SetCurrentDataSource( string DataSourceName )
		{
			foreach( NTwain.DataSource datasource in this._TwainSession )
			{
				if( datasource.Name == DataSourceName )
				{
					try
					{
						NTwain.Data.ReturnCode return_code = datasource.Open();
						if( return_code != NTwain.Data.ReturnCode.Success )
						{
							throw new Exception( "Unable to open Twain DataSource. Return Code was: " + return_code.ToString() );
						}
						this.Fire_SourceLoaded( new SourceLoaded_EventArgs( datasource ) );
					}
					catch( Exception exception )
					{
						this.Fire_Error( new Error_EventArgs( exception ) );
						return false;
					}
					return true;
				}
			}
			return false;
		}


		//=====================================================================
		public bool SetCurrentPageSize( string PageSizeName )
		{
			foreach( NTwain.Data.SupportedSize size in this.PageSizeList )
			{
				if( size.ToString() == PageSizeName )
				{
					this.CurrentPageSize = size;
					return true;
				}
			}
			return false;
		}


		//=====================================================================
		public bool SetCurrentDPI( string DpiName )
		{
			foreach( NTwain.Data.TWFix32 dpi in this.DpiList )
			{
				if( dpi.ToString() == DpiName )
				{
					this.CurrentDpi = dpi;
					return true;
				}
			}
			return false;
		}


		//=====================================================================
		public bool SetCurrentDepth( string DepthName )
		{
			foreach( NTwain.Data.PixelType depth in this.DepthList )
			{
				if( depth.ToString() == DepthName )
				{
					this.CurrentDepth = depth;
					return true;
				}
			}
			return false;
		}


		//=====================================================================
		public bool LoadSourceCapabilities()
		{
			if( this._TwainSession.CurrentSource == null ) { return false; }
			NTwain.Capabilities capabilities = this._TwainSession.CurrentSource.Capabilities;

			this.CurrentPageSize = NTwain.Data.SupportedSize.USLetter;
			this.PageSizeList = null;

			this.CurrentDpi = 300;
			this.DpiList = null;

			this.CurrentDepth = NTwain.Data.PixelType.RGB;
			this.DepthList = null;

			if( capabilities.ICapSupportedSizes.IsSupported )
			{
				try
				{
					this.PageSizeList = new List<NTwain.Data.SupportedSize>();
					foreach( NTwain.Data.SupportedSize size in capabilities.ICapSupportedSizes.GetValues().ToList() )
					{
						this.PageSizeList.Add( size );
					}
					this.CurrentPageSize = capabilities.ICapSupportedSizes.GetCurrent();
				}
				catch( Exception exception )
				{
					this.Fire_Error( new Error_EventArgs( exception ) );
				}
			}

			if( capabilities.ICapXResolution.IsSupported && capabilities.ICapYResolution.IsSupported )
			{
				try
				{
					this.DpiList = new List<NTwain.Data.TWFix32>();
					foreach( NTwain.Data.TWFix32 dpi in capabilities.ICapXResolution.GetValues().ToList() )
					{
						this.DpiList.Add( dpi );
					}
					this.CurrentDpi = capabilities.ICapXResolution.GetCurrent();
				}
				catch( Exception exception )
				{
					this.Fire_Error( new Error_EventArgs( exception ) );
				}
			}

			if( capabilities.ICapPixelType.IsSupported )
			{
				try
				{
					this.DepthList = new List<NTwain.Data.PixelType>();
					foreach( NTwain.Data.PixelType pixel_type in capabilities.ICapPixelType.GetValues().ToList() )
					{
						this.DepthList.Add( pixel_type );
					}
					this.CurrentDepth = capabilities.ICapPixelType.GetCurrent();
				}
				catch( Exception exception )
				{
					this.Fire_Error( new Error_EventArgs( exception ) );
				}
			}

			return true;
		}


		//=====================================================================
		public bool StartCapture( System.Windows.Forms.IWin32Window Window )
		{
			if( this._TwainSession.State != 4 ) { return false; }
			this._StopCapture = false;

			if( this._TwainSession.CurrentSource.Capabilities.ICapSupportedSizes.IsSupported )
			{
				this._TwainSession.CurrentSource.Capabilities.ICapSupportedSizes.SetValue( this.CurrentPageSize );
			}
			if( this._TwainSession.CurrentSource.Capabilities.ICapXResolution.IsSupported
				&& this._TwainSession.CurrentSource.Capabilities.ICapYResolution.IsSupported )
			{
				this._TwainSession.CurrentSource.Capabilities.ICapXResolution.SetValue( this.CurrentDpi );
				this._TwainSession.CurrentSource.Capabilities.ICapYResolution.SetValue( this.CurrentDpi );
			}
			if( this._TwainSession.CurrentSource.Capabilities.ICapPixelType.IsSupported )
			{
				this._TwainSession.CurrentSource.Capabilities.ICapPixelType.SetValue( this.CurrentDepth );
			}

			if( this._TwainSession.CurrentSource.Capabilities.CapUIControllable.IsSupported )//.SupportedCaps.Contains(CapabilityId.CapUIControllable))
			{
				// hide scanner ui if possible
				if( this._TwainSession.CurrentSource.Enable( NTwain.SourceEnableMode.NoUI, false, Window.Handle ) == NTwain.Data.ReturnCode.Success )
				{
					//btnStopScan.Enabled = true;
					//btnStartCapture.Enabled = false;
					//panelOptions.Enabled = false;
				}
			}
			else
			{
				if( this._TwainSession.CurrentSource.Enable( NTwain.SourceEnableMode.ShowUI, true, Window.Handle ) == NTwain.Data.ReturnCode.Success )
				{
					//btnStopScan.Enabled = true;
					//btnStartCapture.Enabled = false;
					//panelOptions.Enabled = false;
				}
			}

			return true;
		}


		//=====================================================================
		public void StopCapture()
		{
			this._StopCapture = true;
			return;
		}


		//=====================================================================
		public static string Text4TwainState( int TwainState )
		{
			// FROM: http://www.twain.org/docs/530fe0da85f7511c510004ff/TWAIN-2.3-Spec.pdf (page 24)
			switch( TwainState )
			{
				case 1:
					return "Pre-Session";
				case 2:
					return "Source Manager Loaded";
				case 3:
					return "Source Manager Opened";
				case 4:
					return "Source Opened";
				case 5:
					return "Source Enabled";
				case 6:
					return "Transfer Ready";
				case 7:
					return "Tranferring";
				default:
					return "UNKNOWN STATE: " + TwainState.ToString();
			}
		}


	}
}

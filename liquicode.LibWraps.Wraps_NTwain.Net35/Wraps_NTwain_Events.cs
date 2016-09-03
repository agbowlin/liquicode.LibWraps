

using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace liquicode.LibWraps
{
	public partial class Wraps_NTwain
	{


		//=====================================================================
		public class LogMessage_EventArgs : EventArgs
		{
			public string Message = "";
			public Exception Exception = null;
			public LogMessage_EventArgs( string Message, Exception Exception )
			{
				this.Message = Message;
				this.Exception = Exception;
				return;
			}
		}
		public delegate void LogMessage_EventHandler( object sender, LogMessage_EventArgs e );
		public event LogMessage_EventHandler Event_LogMessage = null;
		public virtual void Fire_LogMessage( LogMessage_EventArgs e )
		{
			if( this.Event_LogMessage != null )
			{
				this.Event_LogMessage( this, e );
			}
			return;
		}


		//=====================================================================
		public class Error_EventArgs : EventArgs
		{
			public Exception Exception = null;
			public Error_EventArgs( Exception Exception )
			{
				this.Exception = Exception;
				return;
			}
		}
		public delegate void Error_EventHandler( object sender, Error_EventArgs e );
		public event Error_EventHandler Event_Error = null;
		public virtual void Fire_Error( Error_EventArgs e )
		{
			if( this.Event_Error != null )
			{
				this.Event_Error( this, e );
			}
			return;
		}


		//=====================================================================
		public class TransferError_EventArgs : EventArgs
		{
			public Exception Exception = null;
			public TransferError_EventArgs( Exception Exception )
			{
				this.Exception = Exception;
				return;
			}
		}
		public delegate void TransferError_EventHandler( object sender, TransferError_EventArgs e );
		public event TransferError_EventHandler Event_TransferError = null;
		public virtual void Fire_TransferError( TransferError_EventArgs e )
		{
			if( this.Event_TransferError != null )
			{
				this.Event_TransferError( this, e );
			}
			return;
		}


		//=====================================================================
		public class SourceLoaded_EventArgs : EventArgs
		{
			NTwain.DataSource DataSource = null;
			public SourceLoaded_EventArgs( NTwain.DataSource DataSource )
			{
				this.DataSource = DataSource;
				return;
			}
		}
		public delegate void SourceLoaded_EventHandler( object sender, SourceLoaded_EventArgs e );
		public event SourceLoaded_EventHandler Event_SourceLoaded = null;
		public virtual void Fire_SourceLoaded( SourceLoaded_EventArgs e )
		{
			if( this.Event_SourceLoaded != null )
			{
				this.Event_SourceLoaded( this, e );
			}
			return;
		}


		//=====================================================================
		public class SourceDisabled_EventArgs : EventArgs
		{
			public SourceDisabled_EventArgs()
			{
				return;
			}
		}
		public delegate void SourceDisabled_EventHandler( object sender, SourceDisabled_EventArgs e );
		public event SourceDisabled_EventHandler Event_SourceDisabled = null;
		public virtual void Fire_SourceDisabled( SourceDisabled_EventArgs e )
		{
			if( this.Event_SourceDisabled != null )
			{
				this.Event_SourceDisabled( this, e );
			}
			return;
		}


		//=====================================================================
		public class PageScanned_EventArgs : EventArgs
		{
			public Image PageScan = null;
			public PageScanned_EventArgs( Image PageScan )
			{
				this.PageScan = PageScan;
				return;
			}
		}
		public delegate void PageScanned_EventHandler( object sender, PageScanned_EventArgs e );
		public event PageScanned_EventHandler Event_PageScanned = null;
		public virtual void Fire_PageScanned( PageScanned_EventArgs e )
		{
			if( this.Event_PageScanned != null )
			{
				this.Event_PageScanned( this, e );
			}
			return;
		}


	}
}

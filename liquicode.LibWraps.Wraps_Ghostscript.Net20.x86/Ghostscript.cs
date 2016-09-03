

using System;
//using System.Drawing;
using System.Runtime.InteropServices;


namespace liquicode.LibWraps
{
	public static class Ghostscript
	{
		//=====================================================================
		// Wrapper for the Ghostscript DLL/API
		// Adapted from: https://github.com/mephraim/ghostscriptsharp
		//=====================================================================

		//=====================================================================
		//=====================================================================
		//
		//		LOW LEVEL API
		//
		//=====================================================================
		//=====================================================================


		public static class GhostscriptDLL
		{
			//=====================================================================
			// Access to the Ghostscript DLL
			//=====================================================================

			[DllImport( "gsdll32.dll", EntryPoint = "gsapi_new_instance" )]
			private static extern int CreateAPIInstance32( out IntPtr pinstance, IntPtr caller_handle );

			[DllImport( "gsdll32.dll", EntryPoint = "gsapi_init_with_args" )]
			private static extern int InitAPI32( IntPtr instance, int argc, string[] argv );

			[DllImport( "gsdll32.dll", EntryPoint = "gsapi_exit" )]
			private static extern int ExitAPI32( IntPtr instance );

			[DllImport( "gsdll32.dll", EntryPoint = "gsapi_delete_instance" )]
			private static extern void DeleteAPIInstance32( IntPtr instance );

			[DllImport( "gsdll64.dll", EntryPoint = "gsapi_new_instance" )]
			private static extern int CreateAPIInstance64( out IntPtr pinstance, IntPtr caller_handle );

			[DllImport( "gsdll64.dll", EntryPoint = "gsapi_init_with_args" )]
			private static extern int InitAPI64( IntPtr instance, int argc, string[] argv );

			[DllImport( "gsdll64.dll", EntryPoint = "gsapi_exit" )]
			private static extern int ExitAPI64( IntPtr instance );

			[DllImport( "gsdll64.dll", EntryPoint = "gsapi_delete_instance" )]
			private static extern void DeleteAPIInstance64( IntPtr instance );


			//=====================================================================
			/// <summary>
			/// GS can only support a single instance, so we need to bottleneck any multi-threaded systems.
			/// </summary>
			private static object resourceLock32 = new object();


			//=====================================================================
			/// <summary>
			/// Calls the Ghostscript API with a collection of arguments to be passed to it
			/// </summary>
			private static void CallAPI32( string[] args )
			{
				// Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
				IntPtr gsInstancePtr;
				lock( resourceLock32 )
				{
					CreateAPIInstance32( out gsInstancePtr, IntPtr.Zero );
					try
					{
						int result = InitAPI32( gsInstancePtr, args.Length, args );

						if( result < 0 )
						{
							throw new ExternalException( "Ghostscript conversion error", result );
						}
					}
					finally
					{
						Cleanup32( gsInstancePtr );
					}
				}
				return;
			}


			//=====================================================================
			/// <summary>
			/// Frees up the memory used for the API arguments and clears the Ghostscript API instance
			/// </summary>
			private static void Cleanup32( IntPtr gsInstancePtr )
			{
				ExitAPI32( gsInstancePtr );
				DeleteAPIInstance32( gsInstancePtr );
				return;
			}


			//=====================================================================
			/// <summary>
			/// GS can only support a single instance, so we need to bottleneck any multi-threaded systems.
			/// </summary>
			private static object resourceLock64 = new object();


			//=====================================================================
			/// <summary>
			/// Calls the Ghostscript API with a collection of arguments to be passed to it
			/// </summary>
			private static void CallAPI64( string[] args )
			{
				// Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
				IntPtr gsInstancePtr;
				lock( resourceLock64 )
				{
					CreateAPIInstance64( out gsInstancePtr, IntPtr.Zero );
					try
					{
						int result = InitAPI64( gsInstancePtr, args.Length, args );

						if( result < 0 )
						{
							throw new ExternalException( "Ghostscript conversion error", result );
						}
					}
					finally
					{
						Cleanup64( gsInstancePtr );
					}
				}
				return;
			}


			//=====================================================================
			/// <summary>
			/// Frees up the memory used for the API arguments and clears the Ghostscript API instance
			/// </summary>
			private static void Cleanup64( IntPtr gsInstancePtr )
			{
				ExitAPI64( gsInstancePtr );
				DeleteAPIInstance64( gsInstancePtr );
				return;
			}


			//=====================================================================
			public static void CallAPI( string[] args )
			{
				if( IntPtr.Size == 4 )
					CallAPI32( args );
				else
					CallAPI64( args );
				return;
			}


			//=====================================================================
			public static void Cleanup( IntPtr gsInstancePtr )
			{
				if( IntPtr.Size == 4 )
					Cleanup32( gsInstancePtr );
				else
					Cleanup64( gsInstancePtr );
				return;
			}


		}


		//=====================================================================
		//=====================================================================
		//
		//		WRAPPER OBJECTS
		//
		//=====================================================================
		//=====================================================================


		//=====================================================================
		/// <summary>
		/// Ghostscript command
		/// </summary>
		public class GhostscriptCommand
		{
			public string InputPath = "";
			public string OutputPath = "";
			public GhostscriptDevices Device = GhostscriptDevices.jpeg;
			public GhostscriptPages Pages = new GhostscriptPages();
			public System.Drawing.Size Resolution = new System.Drawing.Size();
			public GhostscriptPageSize Size = new GhostscriptPageSize();


			//=====================================================================
			/// <summary>
			/// Returns an array of arguments to be sent to the Ghostscript API
			/// </summary>
			/// <param name="inputPath">Path to the source file</param>
			/// <param name="outputPath">Path to the output file</param>
			/// <param name="settings">API parameters</param>
			/// <returns>API arguments</returns>
			public string[] GetArgs()
			{
				//string[] ARGS = new string[] {
				//    // Keep gs from writing information to standard output
				//    "-q",                     
				//    "-dQUIET",

				//    "-dPARANOIDSAFER",       // Run this command in safe mode
				//    "-dBATCH",               // Keep gs from going into interactive mode
				//    "-dNOPAUSE",             // Do not prompt and pause for each page
				//    "-dNOPROMPT",            // Disable prompts for user interaction           
				//    "-dMaxBitmap=500000000", // Set high for better performance
				//    "-dNumRenderingThreads=4", // Multi-core, come-on!

				//    // Configure the output anti-aliasing, resolution, etc
				//    "-dAlignToPixels=0",
				//    "-dGridFitTT=0",
				//    "-dTextAlphaBits=4",
				//    "-dGraphicsAlphaBits=4"
				//};

				System.Collections.ArrayList arg_list = new System.Collections.ArrayList
				(
					new string[]
					{
						// Keep gs from writing information to standard output
						"-q",                     
						"-dQUIET",
               
						"-dPARANOIDSAFER",       // Run this command in safe mode
						"-dBATCH",               // Keep gs from going into interactive mode
						"-dNOPAUSE",             // Do not prompt and pause for each page
						"-dNOPROMPT",            // Disable prompts for user interaction           
						"-dMaxBitmap=500000000", // Set high for better performance
						"-dNumRenderingThreads=4", // Multi-core, come-on!
                
						// Configure the output anti-aliasing, resolution, etc
						"-dAlignToPixels=0",
						"-dGridFitTT=0",
						"-dTextAlphaBits=4",
						"-dGraphicsAlphaBits=4"
					}
				);

				if( this.Device == GhostscriptDevices.UNDEFINED )
				{
					throw new ArgumentException( "An output device must be defined for Ghostscript", "GhostscriptSettings.Device" );
				}

				if( (this.Pages.AllPages == false)
					&& (this.Pages.Start <= 0)
					&& (this.Pages.End < this.Pages.Start) )
				{
					throw new ArgumentException( "Pages to be printed must be defined.", "GhostscriptSettings.Pages" );
				}

				if( this.Resolution.IsEmpty )
				{
					throw new ArgumentException( "An output resolution must be defined", "GhostscriptSettings.Resolution" );
				}

				if( this.Size.Native == GhostscriptPageSizes.UNDEFINED && this.Size.Manual.IsEmpty )
				{
					throw new ArgumentException( "Page size must be defined", "GhostscriptSettings.Size" );
				}

				// Output device
				arg_list.Add( String.Format( "-sDEVICE={0}", this.Device ) );

				// Pages to output
				if( this.Pages.AllPages )
				{
					arg_list.Add( "-dFirstPage=1" );
				}
				else
				{
					arg_list.Add( String.Format( "-dFirstPage={0}", this.Pages.Start ) );
					if( this.Pages.End >= this.Pages.Start )
					{
						arg_list.Add( String.Format( "-dLastPage={0}", this.Pages.End ) );
					}
				}

				// Page size
				if( this.Size.Native == GhostscriptPageSizes.UNDEFINED )
				{
					arg_list.Add( String.Format( "-dDEVICEWIDTHPOINTS={0}", this.Size.Manual.Width ) );
					arg_list.Add( String.Format( "-dDEVICEHEIGHTPOINTS={0}", this.Size.Manual.Height ) );
					arg_list.Add( "-dFIXEDMEDIA" );
					arg_list.Add( "-dPDFFitPage" );
				}
				else
				{
					arg_list.Add( String.Format( "-sPAPERSIZE={0}", this.Size.Native.ToString() ) );
				}

				// Page resolution
				arg_list.Add( String.Format( "-dDEVICEXRESOLUTION={0}", this.Resolution.Width ) );
				arg_list.Add( String.Format( "-dDEVICEYRESOLUTION={0}", this.Resolution.Height ) );

				// Files
				arg_list.Add( String.Format( "-sOutputFile={0}", this.OutputPath ) );
				arg_list.Add( this.InputPath );

				// Return the settings as an array of strings.
				return (string[])arg_list.ToArray( typeof( string ) );

			}

		}


		//=====================================================================
		/// <summary>
		/// Which pages to output
		/// </summary>
		public class GhostscriptPages
		{
			private bool _allPages = true;
			private int _start;
			private int _end;

			/// <summary>
			/// Output all pages avaialble in document
			/// </summary>
			public bool AllPages
			{
				set
				{
					this._start = -1;
					this._end = -1;
					this._allPages = true;
				}
				get
				{
					return this._allPages;
				}
			}

			/// <summary>
			/// Start output at this page (1 for page 1)
			/// </summary>
			public int Start
			{
				set
				{
					this._allPages = false;
					this._start = value;
				}
				get
				{
					return this._start;
				}
			}

			/// <summary>
			/// Page to stop output at
			/// </summary>
			public int End
			{
				set
				{
					this._allPages = false;
					this._end = value;
				}
				get
				{
					return this._end;
				}
			}
		}


		//=====================================================================
		/// <summary>
		/// Output devices for GhostScript
		/// </summary>
		public enum GhostscriptDevices
		{
			UNDEFINED,
			png16m,
			pnggray,
			png256,
			png16,
			pngmono,
			pngalpha,
			jpeg,
			jpeggray,
			tiffgray,
			tiff12nc,
			tiff24nc,
			tiff32nc,
			tiffsep,
			tiffcrle,
			tiffg3,
			tiffg32d,
			tiffg4,
			tifflzw,
			tiffpack,
			faxg3,
			faxg32d,
			faxg4,
			bmpmono,
			bmpgray,
			bmpsep1,
			bmpsep8,
			bmp16,
			bmp256,
			bmp16m,
			bmp32b,
			pcxmono,
			pcxgray,
			pcx16,
			pcx256,
			pcx24b,
			pcxcmyk,
			psdcmyk,
			psdrgb,
			pdfwrite,
			pswrite,
			epswrite,
			pxlmono,
			pxlcolor
		}


		//=====================================================================
		/// <summary>
		/// Output document physical dimensions
		/// </summary>
		public class GhostscriptPageSize
		{
			private GhostscriptPageSizes _fixed;
			private System.Drawing.Size _manual;

			/// <summary>
			/// Custom document size
			/// </summary>
			public System.Drawing.Size Manual
			{
				set
				{
					this._fixed = GhostscriptPageSizes.UNDEFINED;
					this._manual = value;
				}
				get
				{
					return this._manual;
				}
			}

			/// <summary>
			/// Standard paper size
			/// </summary>
			public GhostscriptPageSizes Native
			{
				set
				{
					this._fixed = value;
					this._manual = new System.Drawing.Size( 0, 0 );
				}
				get
				{
					return this._fixed;
				}
			}

		}


		//=====================================================================
		/// <summary>
		/// Native page sizes
		/// </summary>
		/// <remarks>
		/// Missing 11x17 as enums can't start with a number, and I can't be bothered
		/// to add in logic to handle it - if you need it, do it yourself.
		/// </remarks>
		public enum GhostscriptPageSizes
		{
			UNDEFINED,
			ledger,
			legal,
			letter,
			lettersmall,
			archE,
			archD,
			archC,
			archB,
			archA,
			a0,
			a1,
			a2,
			a3,
			a4,
			a4small,
			a5,
			a6,
			a7,
			a8,
			a9,
			a10,
			isob0,
			isob1,
			isob2,
			isob3,
			isob4,
			isob5,
			isob6,
			c0,
			c1,
			c2,
			c3,
			c4,
			c5,
			c6,
			jisb0,
			jisb1,
			jisb2,
			jisb3,
			jisb4,
			jisb5,
			jisb6,
			b0,
			b1,
			b2,
			b3,
			b4,
			b5,
			flsa,
			flse,
			halfletter
		}


		//=====================================================================
		//=====================================================================
		//
		//		PUBLIC FUNCTIONS
		//
		//=====================================================================
		//=====================================================================


		//=====================================================================
		/// <summary>
		/// Generates a collection of thumbnail jpgs for the pdf at the input path 
		/// starting with firstPage and ending with lastPage.
		/// Put "%d" somewhere in the output path to have each of the pages numbered
		/// </summary>
		public static void GeneratePageThumbs( string inputPath, string outputPath, int firstPage, int lastPage, int dpix, int dpiy, int width, int height, string format = "png" )
		{
			GhostscriptCommand command = new GhostscriptCommand();
			switch( format.ToLower() )
			{
				case "png":
					command.Device = GhostscriptDevices.png16m;
					break;
				case "jpeg":
					command.Device = GhostscriptDevices.jpeg;
					break;
				default:
					command.Device = GhostscriptDevices.UNDEFINED;
					break;
			}
			command.InputPath = inputPath;
			command.OutputPath = outputPath;
			command.Pages.Start = firstPage;
			command.Pages.End = lastPage;
			command.Resolution = new System.Drawing.Size( dpix, dpiy );
			command.Size = new GhostscriptPageSize();
			if( width == 0 && height == 0 )
			{
				command.Size.Native = GhostscriptPageSizes.a7;
			}
			else
			{
				command.Size.Manual = new System.Drawing.Size( width, height );
			}

			GhostscriptDLL.CallAPI( command.GetArgs() );
			return;
		}


		//=====================================================================
		public static void GeneratePageThumb( string inputPath, string outputPath, int page, int dpix, int dpiy, int width, int height, string format = "png" )
		{
			GeneratePageThumbs( inputPath, outputPath, page, page, dpix, dpiy, width, height, format );
			return;
		}


		//=====================================================================
		public static void GeneratePageThumb( string inputPath, string outputPath, int page, int dpix, int dpiy, string format = "png" )
		{
			GeneratePageThumbs( inputPath, outputPath, page, page, dpix, dpiy, 0, 0, format );
			return;
		}


		//=====================================================================
		/// <summary>
		/// Rasterises a PDF into selected format
		/// </summary>
		/// <param name="inputPath">PDF file to convert</param>
		/// <param name="outputPath">Destination file</param>
		/// <param name="command">Conversion settings</param>
		public static void GenerateOutput( string inputPath, string outputPath, GhostscriptCommand command )
		{
			command.InputPath = inputPath;
			command.OutputPath = outputPath;
			GhostscriptDLL.CallAPI( command.GetArgs() );
			return;
		}

	}


}

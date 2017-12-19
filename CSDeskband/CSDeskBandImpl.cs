﻿using System;
using System.Runtime.InteropServices;
using CSDeskband.Interop;
using CSDeskband.Interop.COM;
using System.Drawing;
using static CSDeskband.Interop.DESKBANDINFO.DBIM;
using static CSDeskband.Interop.DESKBANDINFO.DBIMF;
using static CSDeskband.Interop.DESKBANDINFO.DBIF;

namespace CSDeskband
{
    public class CSDeskBandImpl : ICSDeskBand
    {
        public static readonly int S_OK = 0;
        public static readonly int E_NOTIMPL = unchecked((int)0x80004001);
        public static readonly int TASKBAR_DEFAULT_LARGE = 40;
        public static readonly int TASKBAR_DEFAULT_SMALL = 30;

        /// <summary>
        /// Min Size veritcally
        /// </summary>
        public Size MinVertical { get; set; }

        /// <summary>
        /// Max size vertically. int.MaxValue - 1 for no limit
        /// </summary>
        public Size MaxVertical { get; set; }

        /// <summary>
        /// Min size horizontal
        /// </summary>
        public Size MinHorizontal { get; set; }

        /// <summary>
        /// Max size horizontal
        /// </summary>
        public Size MaxHorizontal { get; set; }

        /// <summary>
        /// Ideal size vertically
        /// </summary>
        public Size Vertical { get; set; }

        /// <summary>
        /// Ideal size horizontally
        /// </summary>
        public Size Horizontal { get; set; }

        /// <summary>
        /// Step size for resizing
        /// </summary>
        public int Increment { get; set; } = NO_LIMIT;

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Deskband options
        /// </summary>
        public CSDeskBandOptions Options { get; set; }

        private IntPtr _handle;
        private IInputObjectSite _site;
        private const int NO_LIMIT = int.MaxValue - 1;

        public CSDeskBandImpl(IntPtr handle)
        {
            _handle = handle;
        }

        public int GetWindow(out IntPtr phwnd)
        {
            phwnd = _handle;
            return S_OK;
        }

        public int ContextSensitiveHelp(bool fEnterMode)
        {
            throw new NotImplementedException();
        }

        public int ShowDW([In] bool fShow)
        {
            throw new NotImplementedException();
        }

        public int CloseDW([In] uint dwReserved)
        {
            throw new NotImplementedException();
        }

        public int ResizeBorderDW(RECT prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] IntPtr punkToolbarSite, bool fReserved)
        {
            throw new NotImplementedException();
        }

        public int GetBandInfo(uint dwBandID, DESKBANDINFO.DBIF dwViewMode, ref DESKBANDINFO pdbi)
        {
            if (pdbi.dwMask.HasFlag(DBIM_MINSIZE))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_FLOATING) || dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptMinSize.Y = MinVertical.Width;
                    pdbi.ptMinSize.X = MinVertical.Height;
                }
                else
                {
                    pdbi.ptMinSize.X = MinHorizontal.Width;
                    pdbi.ptMinSize.Y = MinHorizontal.Height;
                }
            }

            if (pdbi.dwMask.HasFlag(DBIM_MAXSIZE))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_FLOATING) || dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptMaxSize.Y = MaxVertical.Width;
                    pdbi.ptMaxSize.X = MaxVertical.Height;
                }
                else
                {
                    pdbi.ptMaxSize.X = MaxHorizontal.Width;
                    pdbi.ptMaxSize.Y = MaxHorizontal.Height;
                }
            }

            // x member is ignored
            if (pdbi.dwMask.HasFlag(DBIM_INTEGRAL))
            {
                pdbi.ptIntegral.Y = Increment;
                pdbi.ptIntegral.X = 0;
            }

            if (pdbi.dwMask.HasFlag(DBIM_ACTUAL))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_FLOATING) || dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptActual.Y = Vertical.Width;
                    pdbi.ptActual.X = Vertical.Height;
                }
                else
                {
                    pdbi.ptActual.X = Vertical.Width;
                    pdbi.ptActual.Y = Vertical.Height;
                }
            }

            if (pdbi.dwMask.HasFlag(DBIM_TITLE))
            {
                pdbi.wszTitle = Title;
            }

            pdbi.dwMask = pdbi.dwMask & ~DBIM_BKCOLOR | DBIM_TITLE;

            if (pdbi.dwMask.HasFlag(DBIM_MODEFLAGS))
            {
                pdbi.dwModeFlags = DBIMF_NORMAL;
                pdbi.dwModeFlags |= Options.AlwaysShowGripper ? DBIMF_ALWAYSGRIPPER : 0;
                pdbi.dwModeFlags |= Options.Fixed ? DBIMF_FIXED | DBIMF_NOGRIPPER : 0;
                pdbi.dwModeFlags |= Options.NoMargins ? DBIMF_NOMARGINS : 0;
                pdbi.dwModeFlags |= Options.Sunken ? DBIMF_DEBOSSED : 0;
                pdbi.dwModeFlags |= Options.Undeleteable ? DBIMF_UNDELETEABLE : 0;
                pdbi.dwModeFlags |= Options.VariableHeight ? DBIMF_VARIABLEHEIGHT : 0;
            }

            return S_OK;
        }

        public int CanRenderComposited(out bool pfCanRenderComposited)
        {
            pfCanRenderComposited = true;
            return S_OK;
        }

        public int SetCompositionState(bool fCompositionEnabled)
        {
            return S_OK;
        }

        public int GetCompositionState(out bool pfCompositionEnabled)
        {
            pfCompositionEnabled = true;
            return S_OK;
        }

        public void SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite)
        {
            if (_site != null)
            {
                Marshal.ReleaseComObject(_site);
            }

            _site = (IInputObjectSite)pUnkSite;
        }

        public void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvSite)
        {
            ppvSite = _site;
        }
    }
}

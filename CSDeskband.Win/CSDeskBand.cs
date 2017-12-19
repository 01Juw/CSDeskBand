﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSDeskband.Interop;
using System.Runtime.InteropServices;

namespace CSDeskband.Win
{
    public class CSDeskBand: UserControl, ICSDeskBand
    {
        public Size MinVertical { get; set; } = new Size(CSDeskBandImpl.TASKBAR_DEFAULT_SMALL, 100);
        public Size MaxVertical { get; set; } = new Size(CSDeskBandImpl.TASKBAR_DEFAULT_SMALL, 100);
        public Size Vertical { get; set; } = new Size(CSDeskBandImpl.TASKBAR_DEFAULT_SMALL, 100);
        public Size MinHorizontal { get; set; } = new Size(100, CSDeskBandImpl.TASKBAR_DEFAULT_SMALL);
        public Size MaxHorizontal { get; set; } = new Size(100, CSDeskBandImpl.TASKBAR_DEFAULT_SMALL);
        public Size Horizontal { get; set; } = new Size(100, CSDeskBandImpl.TASKBAR_DEFAULT_SMALL);
        public int Increment { get; set; } = CSDeskBandImpl.NO_LIMIT;
        public string Title { get; set; } = "";
        public CSDeskBandOptions Options { get; set; } = new CSDeskBandOptions();

        private CSDeskBandImpl _impl;

        public CSDeskBand()
        {
            _impl = new CSDeskBandImpl(Handle)
            {
                MinHorizontal = MinHorizontal,
                MaxHorizontal = MaxHorizontal,
                Horizontal = Horizontal,
                MinVertical = MinVertical,
                MaxVertical = MaxVertical,
                Vertical = Vertical,
                Increment = Increment,
                Title = Title,
                Options = Options,
            };
        }

        public int GetWindow(out IntPtr phwnd)
        {
            return _impl.GetWindow(out phwnd);
        }

        public int ContextSensitiveHelp(bool fEnterMode)
        {
            return _impl.ContextSensitiveHelp(fEnterMode);
        }

        public int ShowDW([In] bool fShow)
        {
            if (fShow)
            {
                Show();
            }
            else
            {
                Hide();
            }
            return _impl.ShowDW(fShow);
        }

        public int CloseDW([In] uint dwReserved)
        {
            Dispose(true);
            return _impl.CloseDW(dwReserved);
        }

        public int ResizeBorderDW(RECT prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] IntPtr punkToolbarSite, bool fReserved)
        {
            return _impl.ResizeBorderDW(prcBorder, punkToolbarSite, fReserved);
        }

        public int GetBandInfo(uint dwBandID, DESKBANDINFO.DBIF dwViewMode, ref DESKBANDINFO pdbi)
        {
            return _impl.GetBandInfo(dwBandID, dwViewMode, ref pdbi);
        }

        public int CanRenderComposited(out bool pfCanRenderComposited)
        {
            return _impl.CanRenderComposited(out pfCanRenderComposited);
        }

        public int SetCompositionState(bool fCompositionEnabled)
        {
            return _impl.SetCompositionState(fCompositionEnabled);
        }

        public int GetCompositionState(out bool pfCompositionEnabled)
        {
            return _impl.GetCompositionState(out pfCompositionEnabled);
        }

        public void SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite)
        {
            _impl.SetSite(pUnkSite);
        }

        public void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvSite)
        {
            _impl.GetSite(ref riid, out ppvSite);
        }

        [ComRegisterFunction]
        private static void Register(Type t)
        {
            CSDeskBandImpl.Register(t);
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t)
        {
            CSDeskBandImpl.Unregister(t);
        }
    }
}
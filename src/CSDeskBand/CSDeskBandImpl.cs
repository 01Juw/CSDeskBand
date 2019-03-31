﻿namespace CSDeskBand
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using CSDeskBand.ContextMenu;
    using CSDeskBand.Interop;
    using static CSDeskBand.Interop.DESKBANDINFO.DBIF;
    using static CSDeskBand.Interop.DESKBANDINFO.DBIM;
    using static CSDeskBand.Interop.DESKBANDINFO.DBIMF;

    /// <summary>
    /// Default implementation for icsdeskband
    /// </summary>
    internal sealed class CSDeskBandImpl : ICSDeskBand
    {
        private readonly IntPtr _handle;
        private readonly Dictionary<uint, DeskBandMenuAction> _contextMenuActions = new Dictionary<uint, DeskBandMenuAction>();
        private IntPtr _parentWindowHandle;
        private object _parentSite; // Has these interfaces: IInputObjectSite, IOleWindow, IOleCommandTarget, IBandSite
        private uint _id;
        private uint _menutStartId = 0;
        private Guid _deskbandCommandGroupId = new Guid("EB0FE172-1A3A-11D0-89B3-00A0C90A90AC"); // Command group id for deskband. Used for IOleCommandTarge.Exec

        /// <summary>
        /// Initializes a new instance of the <see cref="CSDeskBandImpl"/> class
        /// with the handle to the window and the options.
        /// </summary>
        /// <param name="handle">Handle to the deskband window.</param>
        /// <param name="options">Deskband options.</param>
        public CSDeskBandImpl(IntPtr handle, CSDeskBandOptions options)
        {
            _handle = handle;
            Options = options;
            Options.PropertyChanged += Options_PropertyChanged;
        }

        /// <summary>
        /// Occurs when the deskband is closed.
        /// </summary>
        internal event EventHandler Closed;

        /// <summary>
        /// Gets the <see cref="CSDeskBandOptions"/>.
        /// </summary>
        internal CSDeskBandOptions Options { get; }

        /// <summary>
        /// Gets the <see cref="TaskbarInfo"/>.
        /// </summary>
        internal TaskbarInfo TaskbarInfo { get; } = new TaskbarInfo();

        /// <inheritdoc/>
        public int GetWindow(out IntPtr phwnd)
        {
            phwnd = _handle;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int ContextSensitiveHelp(bool fEnterMode)
        {
            return HRESULT.E_NOTIMPL;
        }

        /// <inheritdoc/>
        public int ShowDW([In] bool fShow)
        {
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int CloseDW([In] uint dwReserved)
        {
            Closed?.Invoke(this, null);
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int ResizeBorderDW(RECT prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] IntPtr punkToolbarSite, bool fReserved)
        {
            // Must return notimpl
            return HRESULT.E_NOTIMPL;
        }

        /// <inheritdoc/>
        public int GetBandInfo(uint dwBandID, DESKBANDINFO.DBIF dwViewMode, ref DESKBANDINFO pdbi)
        {
            // Sizing information is requested whenever the taskbar changes size/orientation
            _id = dwBandID;

            if (pdbi.dwMask.HasFlag(DBIM_MINSIZE))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptMinSize.Y = Options.MinVerticalSize.Width;
                    pdbi.ptMinSize.X = Options.MinVerticalSize.Height;
                }
                else
                {
                    pdbi.ptMinSize.X = Options.MinHorizontalSize.Width;
                    pdbi.ptMinSize.Y = Options.MinHorizontalSize.Height;
                }
            }

            // X is ignored
            if (pdbi.dwMask.HasFlag(DBIM_MAXSIZE))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptMaxSize.Y = Options.MaxVerticalWidth;
                    pdbi.ptMaxSize.X = 0;
                }
                else
                {
                    pdbi.ptMaxSize.X = 0;
                    pdbi.ptMaxSize.Y = Options.MaxHorizontalHeight;
                }
            }

            // x member is ignored
            if (pdbi.dwMask.HasFlag(DBIM_INTEGRAL))
            {
                pdbi.ptIntegral.Y = Options.HeightIncrement;
                pdbi.ptIntegral.X = 0;
            }

            if (pdbi.dwMask.HasFlag(DBIM_ACTUAL))
            {
                if (dwViewMode.HasFlag(DBIF_VIEWMODE_VERTICAL))
                {
                    pdbi.ptActual.Y = Options.VerticalSize.Width;
                    pdbi.ptActual.X = Options.VerticalSize.Height;
                }
                else
                {
                    pdbi.ptActual.X = Options.HorizontalSize.Width;
                    pdbi.ptActual.Y = Options.HorizontalSize.Height;
                }
            }

            if (pdbi.dwMask.HasFlag(DBIM_TITLE))
            {
                pdbi.wszTitle = Options.Title;
                if (!Options.ShowTitle)
                {
                    pdbi.dwMask &= ~DBIM_TITLE;
                }
            }

            if (pdbi.dwMask.HasFlag(DBIM_MODEFLAGS))
            {
                pdbi.dwModeFlags = DBIMF_NORMAL;
                pdbi.dwModeFlags |= Options.IsFixed ? DBIMF_FIXED | DBIMF_NOGRIPPER : 0;
                pdbi.dwModeFlags |= Options.HeightCanChange ? DBIMF_VARIABLEHEIGHT : 0;
                pdbi.dwModeFlags &= ~DBIMF_BKCOLOR; // Don't use background color
            }

            TaskbarInfo.UpdateInfo();

            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int CanRenderComposited(out bool pfCanRenderComposited)
        {
            pfCanRenderComposited = true;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int SetCompositionState(bool fCompositionEnabled)
        {
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int GetCompositionState(out bool pfCompositionEnabled)
        {
            pfCompositionEnabled = true;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int SetSite([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSite)
        {
            /**
            1.Release any site pointer that is currently being held.
            2.If the pointer passed to SetSite is set to NULL, the band is being removed. SetSite can return S_OK.
            3.If the pointer passed to SetSite is non-NULL, a new site is being set. SetSite should do the following:
                a.Call QueryInterface on the site for its IOleWindow interface.
                b.Call IOleWindow::GetWindow to obtain the parent window's handle. Save the handle for later use. Release IOleWindow if it is no longer needed.
                c.Create the band object's window as a child of the window obtained in the previous step. Do not create it as a visible window.
                d.If the band object implements IInputObject, call QueryInterface on the site for its IInputObjectSite interface. Store the pointer to this interface for use later.
                e.If all steps are successful, return S_OK. If not, return the OLE-defined error code indicating what failed.
            **/

            if (_parentSite != null)
            {
                Marshal.ReleaseComObject(_parentSite);
            }

            // pUnkSite null means deskband was closed
            if (pUnkSite == null)
            {
                Closed?.Invoke(this, null);
                return HRESULT.S_OK;
            }

            var oleWindow = (IOleWindow)pUnkSite;
            oleWindow.GetWindow(out _parentWindowHandle);
            User32.SetParent(_handle, _parentWindowHandle);
            Marshal.ReleaseComObject(oleWindow);

            _parentSite = (IInputObjectSite)pUnkSite;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out IntPtr ppvSite)
        {
            if (_parentSite != null)
            {
                Marshal.QueryInterface(Marshal.GetIUnknownForObject(_parentSite), ref riid, out ppvSite);
                return HRESULT.S_OK;
            }
            else
            {
                ppvSite = IntPtr.Zero;
                return HRESULT.E_FAIL;
            }
        }

        /// <inheritdoc/>
        public int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, QueryContextMenuFlags uFlags)
        {
            if (uFlags.HasFlag(QueryContextMenuFlags.CMF_DEFAULTONLY))
            {
                return HRESULT.MakeHResult((uint)HRESULT.S_OK, 0, 0);
            }

            _menutStartId = idCmdFirst;
            foreach (var item in Options.ContextMenuItems)
            {
                item.AddToMenu(hMenu, indexMenu++, ref idCmdFirst, _contextMenuActions);
            }

            return HRESULT.MakeHResult((uint)HRESULT.S_OK, 0, idCmdFirst + 1); // #id of last command + 1
        }

        /// <inheritdoc/>
        public int InvokeCommand(IntPtr pici)
        {
            var commandInfo = Marshal.PtrToStructure<CMINVOKECOMMANDINFO>(pici);
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            var isUnicode = false;
            var isExtended = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            var verbPtr = commandInfo.lpVerb;

            if (commandInfo.cbSize == Marshal.SizeOf<CMINVOKECOMMANDINFOEX>())
            {
                isExtended = true;

                var extended = Marshal.PtrToStructure<CMINVOKECOMMANDINFOEX>(pici);
                if (extended.fMask.HasFlag(CMINVOKECOMMANDINFOEX.CMIC.CMIC_MASK_UNICODE))
                {
                    isUnicode = true;
                    verbPtr = extended.lpVerbW;
                }
            }

            if (User32.HiWord(commandInfo.lpVerb.ToInt32()) != 0)
            {
                // TODO verbs
                return HRESULT.E_FAIL;
            }

            var cmdIndex = User32.LoWord(verbPtr.ToInt32());

            if (!_contextMenuActions.TryGetValue((uint)cmdIndex + _menutStartId, out var action))
            {
                return HRESULT.E_FAIL;
            }

            action.DoAction();
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int GetCommandString(ref uint idcmd, uint uflags, ref uint pwReserved, out string pcszName, uint cchMax)
        {
            pcszName = "";
            return HRESULT.E_NOTIMPL;
        }

        /// <inheritdoc/>
        public int HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            return HandleMenuMsg2(uMsg, wParam, lParam, out var i);
        }

        /// <inheritdoc/>
        public int HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, out IntPtr plResult)
        {
            plResult = IntPtr.Zero;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int GetClassID(out Guid pClassID)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int GetSizeMax(out ulong pcbSize)
        {
            pcbSize = 0;
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int IsDirty()
        {
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int Load(object pStm)
        {
            return HRESULT.S_OK;
        }

        /// <inheritdoc/>
        public int Save(object pStm, bool fClearDirty)
        {
            return HRESULT.S_OK;
        }

        /// <summary>
        /// Closes the deskband.
        /// </summary>
        public void CloseDeskBand()
        {
            var bandSite = (IBandSite)_parentSite;
            bandSite.RemoveBand(_id);
        }

        /// <inheritdoc/>
        public void UIActivateIO(int fActivate, ref MSG msg)
        {
            // TODO
        }

        /// <inheritdoc/>
        public int HasFocusIO()
        {
            return HRESULT.E_NOTIMPL;
        }

        /// <inheritdoc/>
        public int TranslateAcceleratorIO(ref MSG msg)
        {
            return HRESULT.E_NOTIMPL;
        }

        private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_parentSite == null)
            {
                return;
            }

            var parent = (IOleCommandTarget)_parentSite;

            // Set pvaln to the id that was passed in SetSite
            // When int is marshalled to variant, it is marshalled as VT_i4. See default marshalling for objects
            parent.Exec(ref _deskbandCommandGroupId, (uint)tagDESKBANDCID.DBID_BANDINFOCHANGED, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }
}

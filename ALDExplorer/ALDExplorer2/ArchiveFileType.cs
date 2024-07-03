﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using FreeImageAPI;
using System.Drawing;
using System.Windows.Forms;
using ZLibNet;

namespace ALDExplorer.ALDExplorer2
{
    public enum ArchiveFileType
    {
        AldFile = 0,
        DatFile,
        AlkFile,
        Afa1File,
        Afa2File,
        SofthouseCharaVfs11File,
        SofthouseCharaVfs20File,
        HoneybeeArcFile,
        BunchOfFiles,
        Invalid = -1,
    }
}

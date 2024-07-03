﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ALDExplorer.ALDExplorer2
{
    public partial class ArchiveFileCollection
    {
        partial void ReadMultipleFiles(string firstArchiveFileName, ref bool success)
        {
            success = false;
            string extension = Path.GetExtension(firstArchiveFileName).ToLowerInvariant();
            if (extension == ".ald")
            {
                string[] allFiles = AldArchiveFile.AldUtil.GetAldOtherFiles(firstArchiveFileName).ToArray();
                if (allFiles.Length == 0)
                {
                    throw new FileNotFoundException("No ALD archive files found", firstArchiveFileName);
                }
                foreach (var fileName in allFiles)
                {
                    var archiveFile = ArchiveFile.ReadArchiveFile(fileName);
                    if (archiveFile == null)
                    {
                        throw new InvalidDataException();
                    }
                    archiveFile.Parent = this;
                    //archiveFile.ReadFile(fileName);
                    ArchiveFiles.Add(archiveFile);
                }
                success = true;
            }
            else if (extension == ".dat")
            {
                string[] allFiles = AldArchiveFile.AldUtil.GetDatOtherFiles(firstArchiveFileName).ToArray();
                if (allFiles.Length == 0)
                {
                    throw new FileNotFoundException("No DAT archive files found", firstArchiveFileName);
                }
                foreach (var fileName in allFiles)
                {
                    var archiveFile = ArchiveFile.ReadArchiveFile(fileName);
                    if (archiveFile == null)
                    {
                        throw new InvalidDataException();
                    }
                    archiveFile.Parent = this;
                    //archiveFile.ReadFile(fileName);
                    ArchiveFiles.Add(archiveFile);
                }
                success = true;
            }
            //else if (!File.Exists(firstArchiveFileName))
            //{
            //    throw new FileNotFoundException("Archive file not found", firstArchiveFileName);
            //}

            if (success)
            {
                var firstArchiveFile = ArchiveFiles.FirstOrDefault() as AldArchiveFile;
                if (firstArchiveFile != null)
                {
                    ReadFileNumbersFromIndexBlock(firstArchiveFile.IndexBlock);
                }
            }
        }

        private void ReadFileNumbersFromIndexBlock(byte[] tableData)
        {
            if (tableData == null || !(this.FileType == ArchiveFileType.AldFile || this.FileType == ArchiveFileType.DatFile))
            {
                return;
            }

            //clear all file numbers
            foreach (var archiveFile in ArchiveFiles)
            {
                foreach (var entry in archiveFile.FileEntries)
                {
                    entry.FileNumber = 0;
                }
            }

            var tableSize = tableData.Length;
            if (this.FileType == ArchiveFileType.AldFile)
            {
                int maxFileNumber = tableSize / 3;

                for (int rawFileNumber = 0; rawFileNumber < maxFileNumber; rawFileNumber++)
                {
                    int fileNumber = rawFileNumber + 1;
                    int entryFileLetter = tableData[rawFileNumber * 3 + 0];
                    int rawFileIndex = tableData[rawFileNumber * 3 + 1] + tableData[rawFileNumber * 3 + 2] * 256;
                    if (rawFileIndex != 0)
                    {
                        var archiveFile = GetArchiveFileByLetter(entryFileLetter, false);
                        if (archiveFile != null)
                        {
                            int aldFileIndex = rawFileIndex - 1;
                            if (aldFileIndex < archiveFile.FileEntries.Count)
                            {
                                archiveFile.FileEntries[aldFileIndex].FileNumber = fileNumber;
                            }
                        }
                    }
                }
            }
            else if (this.FileType == ArchiveFileType.DatFile)
            {
                int maxFileNumber = tableSize / 2;

                for (int rawFileNumber = 0; rawFileNumber < maxFileNumber; rawFileNumber++)
                {
                    int fileNumber = rawFileNumber + 1;
                    int entryFileLetter = tableData[rawFileNumber * 2 + 0];
                    int rawFileIndex = tableData[rawFileNumber * 2 + 1];
                    if (rawFileIndex != 0)
                    {
                        var archiveFile = GetArchiveFileByLetter(entryFileLetter, false);
                        if (archiveFile != null)
                        {
                            int datFileIndex = rawFileIndex - 1;
                            if (datFileIndex < archiveFile.FileEntries.Count)
                            {
                                archiveFile.FileEntries[datFileIndex].FileNumber = fileNumber;
                            }
                        }
                    }
                }
            }
        }

        public void CreatePatchAld(int newNumberForA, int numberForZ)
        {
            var fileType = this.FileType;
            if (fileType == ArchiveFileType.AldFile || fileType == ArchiveFileType.DatFile)
            {
                bool renameA = false;
                ArchiveFile aFile = null, mFile = null, zFile = null;
                aFile = GetArchiveFileByLetter(1, false);
                mFile = GetArchiveFileByLetter(newNumberForA, false);
                zFile = GetArchiveFileByLetter(numberForZ, true);

                if (mFile == null)
                {
                    if (aFile != null)
                    {
                        //move AFILE to MFILE
                        aFile.FileLetter = newNumberForA;
                        foreach (var entry in aFile.FileEntries)
                        {
                            entry.FileLetter = newNumberForA;
                        }
                        mFile = aFile;
                        aFile = null;
                        renameA = true;
                    }
                }
                if (aFile == null)
                {
                    aFile = GetArchiveFileByLetter(1);
                }
                SetPatchFileEntries(zFile);

                zFile.UpdateFileHeaders();
                zFile.SaveTempFile();

                mFile.ArchiveFileName = GetArchiveFileName(mFile.ArchiveFileName, newNumberForA);
                aFile.ArchiveFileName = GetArchiveFileName(aFile.ArchiveFileName, 1);

                var aldFile = aFile as AldArchiveFile;
                if (aldFile != null)
                {
                    aldFile.BuildIndexBlock(GetEntries());
                }
                aFile.UpdateFileHeaders();
                aFile.SaveTempFile();

                zFile.CommitTempFile();
                if (renameA)
                {
                    string mFileName = mFile.ArchiveFileName;
                    string aFileName = aFile.ArchiveFileName;
                    File.Move(aFileName, mFileName);
                }
                aFile.CommitTempFile();
            }
        }

        partial void GetAldArchiveFileName(string fileName, int fileLetter, ref string outputFileName)
        {
            var fileType = this.FileType;
            if (fileType == ArchiveFileType.AldFile)
            {
                outputFileName = AldArchiveFile.AldUtil.GetAldFileName(fileName, fileLetter);
            }
            else if (fileType == ArchiveFileType.DatFile)
            {
                outputFileName = AldArchiveFile.AldUtil.GetDatFileName(fileName, fileLetter);
            }
        }

    }
}

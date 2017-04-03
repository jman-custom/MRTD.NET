﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloWord.DataGroups.Content;
using HelloWord.Infrastructure;
using HelloWord.SmartCard.Reader;

namespace HelloWord.DataGroups
{
    public class DG12 : IDataGroup<DGContent>
    {
        private readonly IBacReader _bacReader;
        private readonly IBinary _fid = new BinaryHex("010C");
        public DG12(IBacReader bacReader)
        {
            _bacReader = bacReader;
        }
        public byte[] Bytes()
        {
            return _bacReader.DGData(_fid).Bytes();
        }

        public DGContent Content()
        {
            return new DGContent(); //_bacReader.DGData(_fid)
        }
    }
}
#!/bin/bash
cd ..
dotnet Shell/Table2BytesNew.dll -q -tables || { echo "AllTable2Bytes Failed."; exit 1; }

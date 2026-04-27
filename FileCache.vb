Imports System.IO
Imports System.Threading
Imports System.Text

Public Class FileCache

    Public bigfile As Boolean = False
    'Public MySize As Integer = 0
    'Public MyCacheSize As Integer = 0
    Public MyListID As Integer
    
    Private debuglog As New ycore.DebugMessage 'debugger

    Public FinalHash As Byte()

    Public bigFileName As String
    Public bigfileStream As New SafeFilestream
    'Public bigFileChangeStream As FileStream
    'Public bigFileChangePath As String
    Public bigFileSize As Int64
    Public bigTargetName As String
    Public bigFileDateBinary As Int64
    'Public bigFileHash As Byte()

    Public index As New MemoryStream
    'Public files As New MemoryStream
    Public count As Integer = 0
    Public complete As Boolean = False
    Public changes As Boolean = False

    Public fileBuffer As Byte()
    Public offset As Integer = 0

    Public Sub Rewind()

        index.Seek(0, SeekOrigin.Begin)
        'files.Seek(0, SeekOrigin.Begin)

    End Sub

    Public Sub clear()

        complete = True
        'count = 0
        'MySize = 0
        Try
            index.Close()
        Catch ex As Exception

        End Try
        Try
            If bigfile Then
                bigfileStream.Close()

            End If
        Catch ex As Exception

        End Try
        'Try
        '    If bigfile Then
        '        bigFileChangeStream.Close()

        '    End If
        'Catch ex As Exception

        'End Try
        'Try
        '    If bigfile Then
        '        Alphaleonis.Win32.Filesystem.File.Delete(bigFileChangePath, True)

        '    End If
        'Catch ex As Exception

        'End Try

        'Try
        '    files.Seek(0, SeekOrigin.Begin)
        '    files.SetLength(0)
        '    files.Close()

        'Catch ex As Exception

        'End Try

        'GC.Collect()

        'Try
        '    TransferList.Close()
        'Catch ex As Exception

        'End Try
        'Try
        '    SendStream.Close()
        'Catch ex As Exception

        'End Try

    End Sub

    'Public Sub Close()

    '    index.Close()
    '    If bigfile Then
    '        bigfileStream.Close()
    '    Else
    '        files.Close()
    '    End If

    'End Sub

    'Public Sub findFileIndex(ByVal offset As Int64, ByVal length As Int64, ByVal debugprefix As String)

    '    If count = 1 Then
    '        debuglog.Send(debugprefix, "finding file special case for single files")
    '        If Not ChangedList.Contains(1) Then ChangedList.Add(1)

    '    ElseIf offset = 0 Then
    '        debuglog.Send(debugprefix, "finding file special case for zero offset")
    '        If Not ChangedList.Contains(1) Then ChangedList.Add(1)

    '    Else
    '        debuglog.Send(debugprefix, "finding file at offset: " & offset & " length: " & length & " count: " & ChangeIndex.Keys.Count)

    '        Dim foundStart As Boolean = False
    '        'Dim removelist As New List(Of Integer)
    '        Dim eof As Int64 = offset + length

    '        For Each changeindexKey As Integer In ChangeIndex.Keys

    '            debuglog.Send(debugprefix, "looking at: " & changeindexKey & " " & ChangeIndex(changeindexKey) & " " & IndexNames(changeindexKey))

    '            If eof <= ChangeIndex(changeindexKey) And foundStart Then
    '                'found the end of file
    '                debuglog.Send(debugprefix, "found the end1 or file already been searched: " & changeindexKey & " " & ChangeIndex(changeindexKey))
    '                Exit For

    '            End If

    '            If foundStart Then
    '                debuglog.Send(debugprefix, "found the middle: " & changeindexKey & " " & ChangeIndex(changeindexKey))
    '                If Not ChangedList.Contains(changeindexKey) Then ChangedList.Add(changeindexKey)

    '            ElseIf offset <= ChangeIndex(changeindexKey) And Not eof < ChangeIndex(changeindexKey) And Not foundStart Then
    '                'If offset = ChangeIndex(changeindexKey) Then
    '                '    'found the start exactly
    '                '    debuglog.Send(debugprefix, "found the start1: " & changeindexKey & " " & ChangeIndex(changeindexKey))
    '                '    If Not ChangedList.Contains(changeindexKey) Then ChangedList.Add(changeindexKey)

    '                'Else
    '                'found the start plus one
    '                debuglog.Send(debugprefix, "found the start2: " & changeindexKey - 1 & " " & ChangeIndex(changeindexKey - 1))
    '                If Not ChangedList.Contains(changeindexKey - 1) Then ChangedList.Add(changeindexKey - 1)

    '                'End If
    '                foundStart = True


    '                If eof <= ChangeIndex(changeindexKey) Then
    '                    'found the end of file
    '                    debuglog.Send(debugprefix, "found the end2 or file: " & changeindexKey & " " & ChangeIndex(changeindexKey))
    '                    Exit For

    '                Else
    '                    debuglog.Send(debugprefix, "found the next one: " & changeindexKey & " " & ChangeIndex(changeindexKey))
    '                    If Not ChangedList.Contains(changeindexKey) Then ChangedList.Add(changeindexKey)

    '                End If

    '            End If

    '            'changeindexKeyPrevious = changeindexKey

    '        Next

    '        'prevent duplicate searches
    '        'If foundStart Then 'only if a change has been made
    '        '    For Each changedListItem As Integer In ChangedList
    '        '        If ChangeIndex.ContainsKey(changedListItem) Then ChangeIndex.Remove(changedListItem)

    '        '    Next

    '        'End If

    '    End If

    '    debuglog.Send(debugprefix, "end finding files")

    'End Sub

    'Public Sub WritetoDisk(ByVal debugprefix As String)

    '    debuglog.Send(debugprefix, "writing to disk " & files.Length & " " & index.Length)

    '    Rewind()

    '    Dim writtenCount As Integer = 1
    '    Dim FileOK As Boolean = True

    '    While writtenCount <= count

    '        If ChangedList.Contains(writtenCount) Then

    '            debuglog.Send(debugprefix, "writing file number " & writtenCount)

    '            Dim int64Buffer(7) As Byte
    '            index.Read(int64Buffer, 0, 8) 'read filesize
    '            Dim remotesize As Int64 = BitConverter.ToInt64(int64Buffer, 0)
    '            'index.Read(int64Buffer, 0, 8) 'read filedate
    '            'Dim remoteDate As Date = Date.FromBinary(BitConverter.ToInt64(int64Buffer, 0))
    '            index.Read(int64Buffer, 0, 4) 'read filename length
    '            Dim filenameLength As Integer = BitConverter.ToInt32(int64Buffer, 0)
    '            Dim buffer(filenameLength - 1) As Byte
    '            index.Read(buffer, 0, filenameLength) 'read filename
    '            Dim targetname As String = Encoding.Unicode.GetString(buffer, 0, filenameLength)

    '            'create buffer
    '            Dim filebuffer(remotesize - 1) As Byte
    '            files.Read(filebuffer, 0, remotesize)

    '            Dim foldername As String = targetname.Remove(targetname.LastIndexOf("\"))

    '            'folder

    '            'debuglog.Send("main","checking folder exists " & "\\?\" & foldername)

    '            'check if directory exisits:
    '            If Not Alphaleonis.Win32.Filesystem.Directory.Exists("\\?\" & foldername) Then
    '                Try
    '                    'create
    '                    debuglog.Send(debugprefix, "creating folder " & "\\?\" & foldername)
    '                    'Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(MyKT, "\\?\" & foldername)
    '                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory("\\?\" & foldername)
    '                    ' dirok = True

    '                Catch ex As Exception
    '                    debuglog.Send(debugprefix, "directory creation error " & ex.Message)
    '                    'RaiseEvent updatelog("Unable to create directory: " & foldername)
    '                    Throw New Exception("Folder error")

    '                End Try

    '            End If

    '            'file

    '            Dim targetFile As IO.FileStream = Nothing
    '            Try
    '                'open
    '                debuglog.Send(debugprefix, "looking for file \\?\" & targetname & " size: " & remotesize)
    '                'targetFile = Alphaleonis.Win32.Filesystem.File.Open(MyKT, "\\?\" & targetname, FileMode.OpenOrCreate, Alphaleonis.Win32.Filesystem.FileAccess.ReadWrite, Alphaleonis.Win32.Filesystem.FileShare.ReadWrite)
    '                'targetFile = Alphaleonis.Win32.Filesystem.File.Open(CType(MyKT, Alphaleonis.Win32.Filesystem.KernelTransaction), "\\?\" & targetname, FileMode.OpenOrCreate, Alphaleonis.Win32.Filesystem.FileAccess.ReadWrite, Alphaleonis.Win32.Filesystem.FileShare.ReadWrite)
    '                targetFile = Alphaleonis.Win32.Filesystem.File.Open("\\?\" & targetname, FileMode.OpenOrCreate, Alphaleonis.Win32.Filesystem.FileAccess.ReadWrite, Alphaleonis.Win32.Filesystem.FileShare.ReadWrite)
    '                'fileok = True
    '                debuglog.Send(debugprefix, "got file " & targetFile.Name & " size: " & targetFile.Length)

    '            Catch ex As Exception
    '                debuglog.Send(debugprefix, "file open error " & ex.Message)
    '                'RaiseEvent updatelog("File access error for: " & targetname)
    '                Throw New Exception("File error")
    '                FileOK = False

    '            End Try

    '            'End If

    '            'If Not fileok Then
    '            '    

    '            'End If
    '            debuglog.Send(debugprefix, "writing " & filebuffer.Length & " to " & targetFile.Position)
    '            targetFile.Write(filebuffer, 0, remotesize)
    '            debuglog.Send(debugprefix, "writen " & filebuffer.Length & " offset now " & targetFile.Position)
    '            targetFile.SetLength(remotesize)
    '            debuglog.Send(debugprefix, "closed file, new length " & targetFile.Length)
    '            targetFile.Close()

    '            'debuglog.Send("main","setting time " & "\\?\" & targetname)
    '            'Alphaleonis.Win32.Filesystem.File.SetLastWriteTimeUtc(MyKT, "\\?\" & targetname, remoteDate)
    '            'debuglog.Send("main","Writing cached file: " & targetname & " size: " & remotesize)

    '        Else
    '            debuglog.Send(debugprefix, "skipping file number " & writtenCount)

    '            Try
    '                Dim int64Buffer(7) As Byte
    '                index.Read(int64Buffer, 0, 8) 'read filesize
    '                Dim remotesize As Int64 = BitConverter.ToInt64(int64Buffer, 0)
    '                'index.Seek(8, IO.SeekOrigin.Current) 'skip filedate
    '                index.Read(int64Buffer, 0, 4) 'read filename length
    '                Dim filenameLength As Integer = BitConverter.ToInt32(int64Buffer, 0)
    '                debuglog.Send(debugprefix, "file details " & remotesize & "/" & filenameLength)

    '                index.Seek(filenameLength, IO.SeekOrigin.Current) 'skip filename

    '                'seek the stream as we are not using this file
    '                files.Seek(remotesize, IO.SeekOrigin.Current)

    '            Catch ex As Exception
    '                debuglog.Send(debugprefix, "write to disk error: " & ex.Message)
    '                Exit While

    '            End Try


    '        End If

    '        writtenCount += 1

    '    End While

    '    debuglog.Send(debugprefix, "Finished writing to disk")

    '    'If FileOK Then
    '    '    Try
    '    '        CType(MyKT, Alphaleonis.Win32.Filesystem.KernelTransaction).Commit()
    '    '        debuglog.Send("main", "Writing to disk Commiting changes ok")

    '    '    Catch ex As Exception
    '    '        debuglog.Send("main", "Writing to disk Commiting changes error" & ex.Message)

    '    '    End Try

    '    'Else
    '    '    Try
    '    '        CType(MyKT, Alphaleonis.Win32.Filesystem.KernelTransaction).Rollback()
    '    '        debuglog.Send("main", "Writing to disk Dismissing changes ok")

    '    '    Catch ex As Exception
    '    '        debuglog.Send("main", "Writing to disk Dismissing changes error" & ex.Message)

    '    '    End Try
    '    '    Throw New Exception("Memory file error transfering")

    '    'End If

    'End Sub

    'Public Sub RecordTransferInfo(ByVal offset As Int64, ByVal size As Int64)

    '    'If offset = MyFileCache.LastOffset + MyFileCache.LastSize And offset > 0 Then
    '    '    debuglog.Send("main","growing change at " & MyFileCache.LastOffset & " by " & size)
    '    '    'consecutive so merge
    '    '    MyFileCache.MySendQueue(MyFileCache.LastOffset) += size
    '    '    MyFileCache.LastSize += size

    '    'Else
    '    debuglog.Send("main", "new change at " & offset & " of " & size)
    '    'non consecutive
    '    Offsets.Enqueue(offset)
    '    Sizes.Enqueue(size)
    '    LastOffset = offset
    '    LastSize = size

    '    'End If

    'End Sub

End Class


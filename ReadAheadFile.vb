Imports System.IO
Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text

Public Class ReadAheadFile

    Dim ChunkCount As Integer = 0
    Dim sourceOffset As Int64 = 0
    Dim sourceStream As SafeFilestream = Nothing
    Dim ActiveBufferA As Boolean = True
    Dim bufferA As Byte()
    Dim bufferB As Byte()
    Dim bufferAready As New ManualResetEvent(False)
    Dim bufferBready As New ManualResetEvent(False)
    Dim bytesRead As Integer
    Dim MyMD5 As MD5
    Dim MySize As Int64 = 0

#If CONFIG = "Debug" Then
    Dim debuglog As New score.DebugMessage
#End If

    Public EOF As Boolean = False
    Dim lastBuffer As Boolean = False

    Public Sub New(ByVal sourceFileIn As SafeFilestream, ByVal chunkSize As Integer, ByVal bufferA_ As Byte(), ByVal bufferB_ As Byte())

        bufferA = bufferA_
        bufferB = bufferB_
        'set the filestream
        sourceStream = sourceFileIn
        MySize = sourceStream.Length

        'create the md5
        MyMD5 = MD5.Create
        'read in the first chunk
#If CONFIG = "Debug" Then
debuglog.Send("readahead", "begin reading file " & chunkSize)
#End If
        sourceStream.SeekBeginRead(sourceOffset, bufferA, 0, chunkSize, AddressOf EndSourceRead, chunkSize)

    End Sub

    'next read is so the length of the chunk being read can be adjusted. It cannot be bigger than the initial chunk size
    Public Function GetBuffer(ByRef mRead As Integer, ByVal NextRead As Integer) As Byte()

        If ActiveBufferA Then
            'wait for buffer to be ready
            bufferAready.WaitOne()
            bufferAready.Reset()
            'return the read value
            mRead = bytesRead
            'change active buffer
            ActiveBufferA = False
            If Not lastBuffer Then
                'set next buffer reading
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "begin reading file B " & NextRead)
#End If
                sourceStream.SeekBeginRead(sourceOffset, bufferB, 0, NextRead, AddressOf EndSourceRead, NextRead)

            Else
                EOF = True

            End If
            'return A
            Return bufferA

        Else
            'wait for buffer to be ready
            bufferBready.WaitOne()
            bufferBready.Reset()
            'return the read value
            mRead = bytesRead
            'change active buffer
            ActiveBufferA = True
            If Not lastBuffer Then
                'set next buffer reading
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "begin reading file A " & NextRead)
#End If
                sourceStream.SeekBeginRead(sourceOffset, bufferA, 0, NextRead, AddressOf EndSourceRead, NextRead)

            Else
                EOF = True

            End If
            'return B
            Return bufferB

        End If

    End Function

    Public Function GetHash() As Byte()

        Return MyMD5.Hash

    End Function

    Private Sub EndSourceRead(ByVal asyncResult As IAsyncResult)

        Dim passedread As Integer = asyncResult.AsyncState

        Try
            Dim claimedread As Integer = sourceStream.EndRead(asyncResult)

            bytesRead = claimedread

        Catch ex As Exception
#If CONFIG = "Debug" Then
            debuglog.Send("readahead", "end read error1: " & ex.Message)
#End If
            'ending the read failed so use the passed read
            bytesRead = passedread

        End Try

        sourceOffset += bytesRead
        ChunkCount += 1

        Try
#If CONFIG = "Debug" Then
            debuglog.Send("readahead", "end read: " & bytesRead & "|" & sourceOffset & "|" & ChunkCount)
#End If
            If sourceStream.Position = MySize Then lastBuffer = True

        Catch ex As Exception
#If CONFIG = "Debug" Then
            debuglog.Send("readahead", "end read error2: " & ex.Message)
#End If
            lastBuffer = True

        End Try

        If ActiveBufferA Then
            ''generate hash
            'Dim hashMD5 As New MD5CryptoServiceProvider
            'Dim myHash As Byte() = hashMD5.ComputeHash(bufferA, 0, bytesRead)
            '#If CONFIG = "Debug" Then
            '            debuglog.Send("readahead", "end reading file A " & chunkSize & " generate md5. Chunk (" & ChunkCount & ") hash: " & Encoding.Unicode.GetString(myHash))
            '#End If
            If lastBuffer Then
                MyMD5.TransformFinalBlock(bufferA, 0, bytesRead)
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "end generate final md5")
#End If
            Else
                MyMD5.TransformBlock(bufferA, 0, bytesRead, Nothing, 0)
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "end generate md5")
#End If
            End If
            bufferAready.Set()

        Else
            ''generate hash
            'Dim hashMD5 As New MD5CryptoServiceProvider
            'Dim myHash As Byte() = hashMD5.ComputeHash(bufferB, 0, bytesRead)
            '#If CONFIG = "Debug" Then
            '            debuglog.Send("readahead", "end reading file B " & chunkSize & " generate md5. Chunk (" & ChunkCount & ") hash: " & Encoding.Unicode.GetString(myHash))
            '#End If
            If lastBuffer Then
                MyMD5.TransformFinalBlock(bufferB, 0, bytesRead)
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "end generate final md5")
#End If
            Else
                MyMD5.TransformBlock(bufferB, 0, bytesRead, Nothing, 0)
#If CONFIG = "Debug" Then
                debuglog.Send("readahead", "end generate md5")
#End If
            End If
            bufferBready.Set()

        End If

    End Sub

    'Public Sub FinishedWait()

    '    If ActiveBufferA Then
    '        'wait for buffer to be ready
    '        bufferAready.WaitOne()
    '        bufferAready.Reset()

    '    Else
    '        'wait for buffer to be ready
    '        bufferBready.WaitOne()
    '        bufferBready.Reset()

    '    End If


    'End Sub

End Class

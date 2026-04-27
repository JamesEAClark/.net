Imports System.IO
Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text

Public Class WriteBehindFile

    Dim chunkcount As Integer = 0
    'Dim chunkSize As Integer
    Dim targetOffset As Int64 = 0
    Dim targetStream As FileStream = Nothing

    Public writingDone As New ManualResetEvent(True)
    Public length As Int64 = 0

#If CONFIG = "Debug" Then
    Dim debuglog As New score.DebugMessage
#End If

    Public Sub New(ByVal targetStreamIn As FileStream)

        'set the filestream
        targetStream = targetStreamIn

    End Sub

    Public Sub WriteBuffer(ByVal bufferIn As Byte(), ByVal offset As Integer, ByVal count As Integer)

        'wait for prior write to finish
        writingDone.WaitOne()
        writingDone.Reset()

        'start writing
#If CONFIG = "Debug" Then
debuglog.Send("readahead", "begin writing file " & count & "/" & targetOffset)
#End If

        targetStream.BeginWrite(bufferIn, offset, count, AddressOf EndWriteBehind, count)

        'increase the file length
        length += count

    End Sub

    Private Sub EndWriteBehind(ByVal endWrite As IAsyncResult)

        Try
            targetStream.EndWrite(endWrite)

            chunkcount += 1

            Dim count As Integer = endWrite.AsyncState
            ''generate hash
            'Dim hashMD5 As New MD5CryptoServiceProvider
            'Dim buffer(count - 1) As Byte
            ''rewind file
            'targetStream.Seek(targetStream.Position - count, SeekOrigin.Begin)
            ''read in block that was just written
            'targetStream.Read(buffer, 0, count)
            'Dim myHash As Byte() = hashMD5.ComputeHash(buffer, 0, count)

#If CONFIG = "Debug" Then
            debuglog.Send("readahead", "end writing file " & length & "/" & count & "/" & targetOffset) ' & " Chunk (" & chunkcount & ") hash: " & Encoding.Unicode.GetString(myHash))
#End If

            targetOffset += count

        Catch ex As Exception
#If CONFIG = "Debug" Then
            debuglog.Send("readahead", "end writing file error " & length & "/" & targetOffset & "|" & ex.Message) ' & " Chunk (" & chunkcount & ") hash: " & Encoding.Unicode.GetString(myHash))
#End If

        End Try

        writingDone.Set()

    End Sub

End Class

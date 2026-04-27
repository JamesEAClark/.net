Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic

Public Class MemoryChunkStream : Inherits Stream

    'MemoryTributary is a re-implementation of MemoryStream that uses a dynamic list of byte arrays as a backing store, instead of a single byte array, the allocation
    'of which will fail for relatively small streams as it requires contiguous memory.

    'converted from c# on code project, commented out bits I don't use for now
    'reference http://www.codeproject.com/script/Articles/ViewDownloads.aspx?aid=348590

#Region "Constructors"

    Public Sub New()

        Position = 0

    End Sub

    'Public Sub New(ByVal source As Byte())

    '    Me.Write(source, 0, source.Length)
    '    Position = 0

    'End Sub

    ''length is ignored because capacity has no meaning unless we implement an artifical limit
    'Public Sub New(ByVal length As Integer)

    '    SetLength(length)
    '    Position = length
    '    d = block   'access block to prompt the allocation of memory
    '    Position = 0

    'End Sub

#End Region

#Region "Status Properties"

    Public Overrides ReadOnly Property CanRead As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property CanSeek As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property CanWrite As Boolean
        Get
            Return True
        End Get
    End Property

#End Region

#Region "Public Properties"

    Public Overrides ReadOnly Property Length As Long
        Get
            Return lengthInt
        End Get
    End Property

    Public Overrides Property Position As Long

#End Region

#Region "Members"

    Protected lengthInt As Long = 0

    Protected blocksize As Long = 65536

    Protected blocks As New List(Of Byte())

#End Region

#Region "Internal Properties"

    'Use these properties to gain access to the appropriate block of memory for the current Position

    'The block of memory currently addressed by Position
    Protected ReadOnly Property block As Byte()
        Get
            While blocks.Count <= blockId
                Dim newBuffer(blocksize - 1) As Byte
                blocks.Add(newBuffer)
            End While
            Return blocks(CInt(blockId))
        End Get
    End Property

    'The id of the block currently addressed by Position
    Protected ReadOnly Property blockId As Long
        Get
            Return Position / blocksize
        End Get
    End Property

    'The offset of the byte currently addressed by Position, into the block that contains it
    Protected ReadOnly Property blockOffset As Long
        Get
            Return Position Mod blocksize
        End Get
    End Property

#End Region

#Region "Public Stream Methods"

    Public Overrides Sub Flush()

    End Sub

    Public Overrides Function Read(bufferIn() As Byte, offset As Integer, count As Integer) As Integer

        Dim lcount As Long = CLng(count)

        If lcount < 0 Then Throw New ArgumentOutOfRangeException("count", lcount, "Number of bytes to copy cannot be negative.")

        Dim remaining As Long = Length - Position

        If lcount > remaining Then lcount = remaining

        If bufferIn Is Nothing Then Throw New ArgumentNullException("buffer", "Buffer cannot be null.")

        If offset < 0 Then Throw New ArgumentOutOfRangeException("offset", offset, "Destination offset cannot be negative.")

        Dim readInt As Integer = 0
        Dim copysize As Long = 0

        Do While lcount > 0

            copysize = Math.Min(lcount, blocksize - blockOffset)
            Buffer.BlockCopy(block, CInt(blockOffset), bufferIn, offset, CInt(copysize))
            lcount -= copysize
            offset += CInt(copysize)

            readInt += CInt(copysize)

            Position += copysize

        Loop

        Return readInt

    End Function

    Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long

        Select Case origin
            Case SeekOrigin.Begin
                Position = offset
            Case SeekOrigin.Current
                Position += offset
            Case SeekOrigin.End
                Position = Length - offset

        End Select
        Return Position

    End Function

    Public Overrides Sub SetLength(value As Long)

        lengthInt = value

    End Sub

    Public Overrides Sub Write(bufferIn() As Byte, offset As Integer, count As Integer)

        Dim initialPosition As Long = Position
        Dim copysize As Integer = 0
        Try
            Do While count > 0
                copysize = Math.Min(count, CInt(blocksize - blockOffset))
                EnsureCapacity(Position + copysize)

                Buffer.BlockCopy(bufferIn, CInt(offset), block, CInt(blockOffset), copysize)
                count -= copysize
                offset += copysize

                Position += copysize

            Loop

        Catch ex As Exception
            Position = initialPosition
            Throw ex

        End Try

    End Sub

    Public Overrides Function ReadByte() As Integer

        If Position >= Length Then Return -1

        Dim b As Byte = block(blockOffset)
        Position += 1

        Return b

    End Function

    Public Overrides Sub WriteByte(ByVal value As Byte)

        EnsureCapacity(Position + 1)
        block(blockOffset) = value
        Position += 1

    End Sub

    Protected Sub EnsureCapacity(ByVal intendedLength As Long)
        If intendedLength > lengthInt Then
            lengthInt = intendedLength
        End If
    End Sub

    Public Sub smelly()


    End Sub

#End Region

#Region "IDispose"

    'http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx
    Protected Overrides Sub Dispose(disposing As Boolean)
        'We do not currently use unmanaged resources
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region "Public Additional Helper Methods"

    'Returns the entire content of the stream as a byte array. This is not safe because the call to new byte[] may 
    'fail if the stream is large enough. Where possible use methods which operate on streams directly instead.
    '<returns>A byte[] containing the current data in the stream</returns>

    Public Function ToArray() As Byte()

        Dim firstposition As Long = Position
        Position = 0
        Dim destination(Length) As Byte
        Read(destination, 0, CInt(Length))
        Position = firstposition
        Return destination

    End Function

    'Reads length bytes from source into the this instance at the current position.
    '<param name="source">The stream containing the data to copy
    '<param name="length">The number of bytes to copy

    'public void ReadFrom(Stream source, long length)
    '{
    '     byte[] buffer = new byte[4096];
    '     int read;
    '     do
    '     {
    '        read = source.Read(buffer, 0, (int)Math.Min(4096, length));
    '        length -= read;
    '        this.Write(buffer, 0, read);

    '     } while (length > 0);
    '}

    'Writes the entire stream into destination, regardless of Position, which remains unchanged.
    '<param name="destination">The stream to write the content of this stream to</param>

    'public void WriteTo(Stream destination)
    '{
    '     long initialpos = Position;
    '     Position = 0;
    '     this.CopyTo(destination);
    '     Position = initialpos;
    '}

#End Region

End Class

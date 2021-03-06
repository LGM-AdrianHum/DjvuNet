// <copyright file="SjbzChunk.cs" company="">
// TODO: Update copyright text.
// </copyright>

using System.Linq;
using DjvuNet.JB2;

namespace DjvuNet.DataChunks
{

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SjbzChunk : DjvuNode, ISjbzChunk
    {

        #region Public Properties

        #region ChunkType

        public override ChunkType ChunkType
        {
            get { return ChunkType.Sjbz; }
        }

        #endregion ChunkType

        #region Image

        private JB2Image _image;

        /// <summary>
        /// Gets the image this chunk represents
        /// </summary>
        public JB2Image Image
        {
            get
            {
                if (_image == null)
                    _image = ReadCompressedImage();

                return _image;
            }

            private set
            {
                if (_image != value)
                    _image = value;
            }
        }

        #endregion Image

        #endregion Public Properties

        #region Constructors

        public SjbzChunk(IDjvuReader reader, IDjvuElement parent, IDjvuDocument document,
            string chunkID = "", long length = 0)
            : base(reader, parent, document, chunkID, length)
        {
        }

        public SjbzChunk(IDjvuWriter writer, IDjvuElement parent, long length = 0)
            : base(writer, parent, length)
        {
        }

        #endregion Constructors

        #region Methods

        internal JB2Image ReadCompressedImage()
        {
            using (IDjvuReader reader = Reader.CloneReaderToMemory(DataOffset, Length))
            {
                JB2Image image = new JB2Image();
                JB2.JB2Dictionary includedDictionary = null;
                DjvuChunk djvuChunk = Parent as DjvuChunk;

                if (djvuChunk != null)
                {
                    var includes = djvuChunk.IncludedItems;

                    if (includes?.Count > 0)
                    {
                        string includeID = includes
                            .FirstOrDefault<InclChunk>(x => x.ChunkType == ChunkType.Incl)?.IncludeID;
                        DjvmChunk root = Document.RootForm as DjvmChunk;
                        DirmComponent component = root?.Dirm.Components
                            .Where<DirmComponent>(x => x.ID == includeID).FirstOrDefault();

                        var includeForm =
                            root.Includes
                            .Where(x =>  x.DataOffset == (component.Offset + 12))
                            .FirstOrDefault<IDjviChunk>();

                        var djbzItem = includeForm?.Children
                            .Where<IDjvuNode>(x => x.ChunkType == ChunkType.Djbz).FirstOrDefault() as DjbzChunk;

                        includedDictionary = djbzItem?.ShapeDictionary;
                    }
                }

                image.Decode(reader, includedDictionary);

                return image;
            }
        }

        #endregion Methods
    }
}

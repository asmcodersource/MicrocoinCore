using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;


namespace Microcoin.JsonStreamParser
{
    /// <summary>
    /// This thing simply reads data from a stream and tries to parse it as a Json serialized object. 
    /// This kind of thing has already been used before, but now it is implemented better (it seems to me).
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    /// Should be base for chain storaging, and transfers via network
    public class JsonStreamParser<Type>
    {
        char[] readBuffer = new char[1024*16];
        protected List<Type> parsedObjects = null;
        protected StringBuilder dataBuffer = new StringBuilder();

        public async Task<List<Type>> ParseJsonObjects(Stream jsonStream, CancellationToken cancellationToken)
        {
            StreamReader reader = new StreamReader(jsonStream);
            parsedObjects = new List<Type>();
            while ( reader.EndOfStream is not true )
            {
                var receivedSize = await reader.ReadAsync(readBuffer, cancellationToken);
                ParsePart(readBuffer, receivedSize);
                if (parsedObjects.Count() != 0)
                    break;
            }
            return parsedObjects;
        }

        protected void ParsePart(char[] chars, int length)
        {
            for( int  i = 0; i < length; i++ )
            {
                // read each symbol, and add it to buffer
                var symbol = chars[i];
                dataBuffer.Append(symbol);
                if (symbol != '}')
                    continue;
                // symbol is '}', then it can be end of json serialized object
                try
                {
                    var streamPart = dataBuffer.ToString();
                    var parsedObject = JsonConvert.DeserializeObject<Type>(streamPart);
                    parsedObjects.Add(parsedObject);
                    dataBuffer.Clear();
                }
                catch (JsonSerializationException) { /* This part isn't correct json */ }
            }
        }
    }
}

using IHI.Server.Install;

namespace IHI.Server
{
    public class CoreManager
    {
        /// <summary>
        ///   The instance of the Server Core
        /// </summary>
        public static ServerCore ServerCore { get; private set; }

        internal static void InitialiseServerCore()
        {
            ServerCore = new ServerCore();
        }
    }
}

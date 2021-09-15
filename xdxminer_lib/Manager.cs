using xdxminer_lib.util;

namespace xdxminer_lib
{
    public class Manager {

        Logger logger = new Logger("m");
        public Stratum clientStratum { get; }
        DirectXMiner directXMiner;
        public Manager()
        {
            
            clientStratum = new Stratum();
            directXMiner = new DirectXMiner();
        }

        public void start ()
        {
            clientStratum.start();
            directXMiner.start();
        }

        public void stop ()
        {
            clientStratum.stop();
            directXMiner.stop();
        }
    }
}

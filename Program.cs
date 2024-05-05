namespace CSharpThreading
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calling ThreadTest1");
            ThreadTest1();
            Console.WriteLine("ThreadTest1 finished\n");

            Console.WriteLine("Calling ThreadTest2");
            ThreadTest2();
            Console.WriteLine("ThreadTest2 finished\n");
        }

        public static void ThreadTest1()
        {
            Console.WriteLine("Before start thread");

            ThreadTest thr1 = new ThreadTest();
            ThreadTest thr2 = new ThreadTest();

            //create some threads and set the ThreadStart methods from the above class
            Thread tid1 = new Thread(new ThreadStart(thr1.Thread1));
            Thread tid2 = new Thread(new ThreadStart(thr2.Thread1));

            tid1.Name = "Thread One";
            tid2.Name = "Thread Two";

            //tid1.IsBackground = true;
            //tid2.IsBackground = true;

            //tid1.Priority = ThreadPriority.Lowest;
            //tid2.Priority = ThreadPriority.Highest;

            try
            {
                tid1.Start();
            }
            catch (ThreadStateException te)
            {
                Console.WriteLine(te.ToString());
            }

            // This prevents the calling thread from calling tid2.Start() for 2 seconds
            // This means Thread One gets a 2 second jump start on processing
            Console.WriteLine($"{tid1.Name} calling Join(2000) ...");
            tid1.Join(2000);
            Console.WriteLine($"After {tid1.Name} called Join(2000)");

            try
            {
                tid2.Start();
            }
            catch (ThreadStateException te)
            {
                Console.WriteLine(te.ToString());
            }

            // This prevents the calling thread from doing anything else until Thread Two terminates
            Console.WriteLine($"{tid2.Name} calling Join() ...");
            tid2.Join();
            Console.WriteLine($"After {tid2.Name} called Join()");

            Console.WriteLine("End of Main");
        }

        public static void ThreadTest2()
        {
            Thread[] myThreads =
                {
                    new Thread(new ThreadStart(Decrementer)),
                    new Thread(new ThreadStart(Incrementer)),
                    new Thread(new ThreadStart(Incrementer))
                };


            int ctr = 1;
            foreach (Thread myThread in myThreads)
            {
                myThread.IsBackground = true;
                myThread.Start();
                myThread.Name = "Thread" + ctr.ToString();
                ctr++;
                Console.WriteLine("Started thread {0}", myThread.Name);
                Thread.Sleep(50);
            }

            myThreads[1].Interrupt();

            foreach (Thread myThread in myThreads)
            {
                myThread.Join();
            }

            Console.WriteLine("All my threads are done.");
        }

        public static void Decrementer()
        {
            try
            {
                for (int i = 1000; i >= 0; i--)
                {
                    Console.WriteLine("Thread {0}. Decrementer: {1}", Thread.CurrentThread.Name, i);
                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Thread {0} interrupted! Cleaning up...", Thread.CurrentThread.Name);
            }
            finally
            {
                Console.WriteLine("Thread {0} Exiting.", Thread.CurrentThread.Name);
            }
        }

        public static void Incrementer()
        {
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    Console.WriteLine("Thread {0}. Incrementer: {1}", Thread.CurrentThread.Name, i);
                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Thread {0} interrupted! Cleaning up...", Thread.CurrentThread.Name);
            }
            finally
            {
                Console.WriteLine("Thread {0} Exiting.", Thread.CurrentThread.Name);
            }
        }
    }

    public class ThreadTest
    {

        //ThreadStart method for tid1 below
        public void Thread1()
        {
            for (int i = 10; i >= 0; i--)
            {
                Thread thr = Thread.CurrentThread;
                Console.WriteLine(thr.Name + " {0}", i);

                try
                {
                    Thread.Sleep(i * 100);
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine(ae.Message);
                }
            }
        }

        //this is currently not used
        public void Thread2()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread thr = Thread.CurrentThread;
                Console.WriteLine(thr.Name + " {0}", i);
            }
        }
    }
}

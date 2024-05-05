using System.Diagnostics.Metrics;

namespace CSharpThreading
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calling ThreadTest1 (no locking of for loop decrementer)");
            ThreadTest1.Run();
            Console.WriteLine("ThreadTest1 finished\n");

            Console.WriteLine("Calling ThreadTest2 (no locking of incrementer)");
            ThreadTest2.Run();
            Console.WriteLine("ThreadTest2 finished\n");

            Console.WriteLine("Calling ThreadTest3 (using lock keyword on incrementer)");
            ThreadTest3.Run();
            Console.WriteLine("ThreadTest3 finished\n");

            Console.WriteLine("Calling ThreadTest4 (using Interlocked.Increment)");
            ThreadTest4.Run();
            Console.WriteLine("ThreadTest4 finished\n");

            Console.WriteLine("Calling ThreadTest5 (using Mutex)");
            ThreadTest5.Run();
            Console.WriteLine("ThreadTest5 finished\n");

            Console.WriteLine("Calling ThreadTest6 (using Monitor)");
            ThreadTest6.Run();
            Console.WriteLine("ThreadTest6 finished\n");

            // Monitor
            // Spinlock
        }

        public static class ThreadTest1
        {
            public static void Run()
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
                Console.WriteLine($"Calling Join(2000) on {tid1.Name} (main thread waits ...)");
                tid1.Join(2000);
                Console.WriteLine($"After calling Join(2000) on {tid1.Name}");

                try
                {
                    tid2.Start();
                }
                catch (ThreadStateException te)
                {
                    Console.WriteLine(te.ToString());
                }

                // This prevents the calling thread from doing anything else until Thread Two terminates
                Console.WriteLine($"Calling Join() on {tid2.Name} (main thread waits ...)");
                tid2.Join();
                Console.WriteLine($"After calling Join() on {tid2.Name}");

                Console.WriteLine("End of Main");
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

        public static class ThreadTest2
        {
            public static void Run()
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
                    for (int i = 100; i >= 0; i--)
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
                    for (int i = 0; i < 100; i++)
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

        public static class ThreadTest3
        {
            private static readonly object _lock = new object();
            private static int counter = 0;

            public static void Run()
            {
                Thread t1 = new Thread(new ThreadStart(Incrementer));
                t1.IsBackground = true;
                t1.Name = "Thread One";
                t1.Start();
                Console.WriteLine("Started thread {0}", t1.Name);

                Thread t2 = new Thread(new ThreadStart(Incrementer));
                t2.IsBackground = true;
                t2.Name = "Thread Two";
                t2.Start();
                Console.WriteLine("Started thread {0}", t2.Name);

                t1.Join();
                t2.Join();

                Console.WriteLine("All my threads are done.");
            }

            public static void Incrementer()
            {
                try
                {
                    while (counter < 100)
                    {
                        lock (_lock)
                        {
                            // Simulate the thread doing some work with the resource (incrementing counter value)
                            int temp = counter;
                            temp++;

                            Thread.Sleep(1);
                            counter = temp;
                        }
                        Console.WriteLine("Thread {0}. Incrementer: {1}", Thread.CurrentThread.Name, counter);
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

        public static class ThreadTest4
        {
            private static int counter = 0;

            public static void Run()
            {
                Thread t1 = new Thread(new ThreadStart(Incrementer));
                t1.IsBackground = true;
                t1.Name = "Thread One";
                t1.Start();
                Console.WriteLine("Started thread {0}", t1.Name);

                Thread t2 = new Thread(new ThreadStart(Incrementer));
                t2.IsBackground = true;
                t2.Name = "Thread Two";
                t2.Start();
                Console.WriteLine("Started thread {0}", t2.Name);

                t1.Join();
                t2.Join();

                Console.WriteLine("All my threads are done.");
            }

            public static void Incrementer()
            {
                try
                {
                    while (counter < 100)
                    {
                        Interlocked.Increment(ref counter);
                        Console.WriteLine("Thread {0}. Incrementer: {1}", Thread.CurrentThread.Name, counter);

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

        public static class ThreadTest5
        {
            // Create a new Mutex. The creating thread does not own the
            // Mutex.
            private static Mutex mutex = new Mutex();
            private static int counter = 0;
            private const int numIterations = 10;
            private const int numThreads = 3;

            public static void Run()
            {
                // Create the threads that will use the protected resource.
                for (int i = 0; i < numThreads; i++)
                {
                    Thread myThread = new Thread(new ThreadStart(MyThreadProc));
                    myThread.Name = String.Format("Thread{0}", i + 1);
                    myThread.Start();
                }

                // The main thread exits, but the application continues to
                // run until all foreground threads have exited.
            }

            private static void MyThreadProc()
            {
                for (int i = 0; i < numIterations; i++)
                {
                    UseResource();
                }
            }

            // This method represents a resource that must be synchronized
            // so that only one thread at a time can enter.
            private static void UseResource()
            {
                // Wait until it is safe to enter:
                // Blocks the current thread until the current WaitHandle receives a signal
                mutex.WaitOne();

                Console.WriteLine("{0} has entered the protected area",
                    Thread.CurrentThread.Name);

                // Place code to access non-reentrant resources here.
                counter++;
                Console.WriteLine($"Counter incremented by {Thread.CurrentThread.Name} to {counter}");

                // Simulate some work.
                Thread.Sleep(500);

                Console.WriteLine("{0} is leaving the protected area\r\n",
                    Thread.CurrentThread.Name);

                // Release the Mutex.
                mutex.ReleaseMutex();
            }
        }

        public static class ThreadTest6
        {
            private static object lockObject = new object();
            private static int counter = 0;

            public  static void Run()
            {
                Thread[] myThreads =
                {
                    new Thread(new ThreadStart(Decrementer)),
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
                    // Synchronize this area of code
                    Monitor.Enter(lockObject);

                    while (counter < 5)
                    {
                        Console.WriteLine("[{0}] In Decrementer. Counter: {1}. Gotta Wait!",
                            Thread.CurrentThread.Name, counter);
                        Monitor.Wait(lockObject);
                    }

                    while (counter > 0)
                    {
                        int temp = counter;
                        temp--;
                        Thread.Sleep(1);
                        counter = temp;
                        Console.WriteLine("[{0}] In Decrementer. Counter: {1}", Thread.CurrentThread.Name, counter);
                    }
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
            }

            public static void Incrementer()
            {
                try
                {
                    //synchronize this area of code
                    //Monitor.Enter(this);

                    while (counter < 10)
                    {
                        Monitor.Enter(lockObject);
                        int temp = counter;
                        temp++;
                        Thread.Sleep(1);
                        counter = temp;
                        Console.WriteLine("[{0}] In Incrementer. Counter: {1}", Thread.CurrentThread.Name, counter);

                        Console.WriteLine($"{Thread.CurrentThread.Name} is pulsing the Monitor");
                        Monitor.Pulse(lockObject);
                        Monitor.Exit(lockObject);
                    }

                    //let another thread have the monitor. I'm done for now.
                    //Monitor.Pulse(this);
                }
                finally
                {
                    //Console.WriteLine("[{0}] Exiting...", Thread.CurrentThread.Name);
                    //Monitor.Exit(this);
                }
            }
        }
    }
}

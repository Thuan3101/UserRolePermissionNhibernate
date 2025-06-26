using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;

namespace SalesManagement.Nhibernate
{
    public class SessionManager
    {
        private static ISessionFactory sessionFactory;
        private static readonly object lockObject = new object();

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (sessionFactory == null)
                {
                    InitSessionFactory();
                }
                return sessionFactory;
            }
        }

        public static ISession CurrentSession
        {
            get
            {
                if (!CurrentSessionContext.HasBind(SessionFactory))
                {
                    var session = SessionFactory.OpenSession();
                    session.BeginTransaction();
                    CurrentSessionContext.Bind(session);
                }

                return SessionFactory.GetCurrentSession();
            }
        }

        public static ISession NewIndependentSession => SessionFactory.OpenSession();

        public static void CommitAndReleaseSession()
        {
            if (CurrentSessionContext.HasBind(SessionFactory))
            {
                ISession session = CurrentSessionContext.Unbind(SessionFactory);
                if (session != null && session.IsOpen)
                {
                    try
                    {
                        if (session.Transaction?.IsActive == true)
                        {
                            session.Flush();
                            session.Transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        session.Transaction?.Rollback();
                        throw;
                    }
                    finally
                    {
                        session.Close();
                    }
                }
            }
        }

        public static void RollbackAndReleaseSession()
        {
            if (CurrentSessionContext.HasBind(SessionFactory))
            {
                ISession session = CurrentSessionContext.Unbind(SessionFactory);
                if (session != null && session.IsOpen)
                {
                    session.Transaction?.Rollback();
                    session.Close();
                }
            }
        }

        private static void InitSessionFactory()
        {
            lock (lockObject)
            {
                if (sessionFactory == null)
                {
                    try
                    {
                        var cfg = new Configuration();
                        cfg.Configure();
                        cfg.AddAssembly(typeof(SessionManager).Assembly);

                        // Set the correct session context class
                        cfg.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass,
                            typeof(NHibernate.Context.AsyncLocalSessionContext).FullName);

                        sessionFactory = cfg.BuildSessionFactory();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("NHibernate initialization failed", ex);
                    }
                }
            }
        }
    }
}
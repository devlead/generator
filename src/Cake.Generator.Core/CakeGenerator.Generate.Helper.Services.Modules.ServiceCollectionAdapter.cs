namespace Cake.Generator;

public partial class CakeGenerator
{
    private const string ServiceCollectionAdapter =
        """
                    private sealed class ServiceRegistration : ICakeRegistrationBuilder
                    {
                        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
                        public Type ImplementationType { get; }
                        public object? Instance { get; set; }
                        public Type? ServiceType { get; set; }
                        public ServiceLifetime Lifetime { get; set; }

                        public ServiceRegistration([System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
                        {
                            ImplementationType = implementationType;
                        }

                        public ICakeRegistrationBuilder As(Type type)
                        {
                            ServiceType = type;
                            return this;
                        }

                        public ICakeRegistrationBuilder AsSelf()
                        {
                            ServiceType = ImplementationType;
                            return this;
                        }

                        public ICakeRegistrationBuilder Singleton()
                        {
                            Lifetime = ServiceLifetime.Singleton;
                            return this;
                        }

                        public ICakeRegistrationBuilder Transient()
                        {
                            Lifetime = ServiceLifetime.Transient;
                            return this;
                        }
                    }

                    private sealed class ServiceCollectionAdapter : ICakeContainerRegistrar
                    {
                        private readonly List<ServiceRegistration> _registrations;

                        public ServiceCollectionAdapter()
                        {
                            _registrations = new List<ServiceRegistration>();
                        }

                        public ICakeRegistrationBuilder RegisterInstance<TImplementation>(TImplementation instance)
                            where TImplementation : class
                        {
            #pragma warning disable IL2087 // 'TImplementation' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicConstructors' in call to 'ServiceRegistration.ServiceRegistration(Type)'
                            var registration = new ServiceRegistration(typeof(TImplementation))
                            {
                                Instance = instance,
                                Lifetime = ServiceLifetime.Singleton,
                                ServiceType = typeof(TImplementation),
                            };
            #pragma warning restore IL2087

                            _registrations.Add(registration);
                            return registration;
                        }

                        public ICakeRegistrationBuilder RegisterType(Type type)
                        {
            #pragma warning disable IL2067 // 'type' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicConstructors' in call to 'ServiceRegistration.ServiceRegistration(Type)'
                            var registration = new ServiceRegistration(type)
                            {
                                Lifetime = ServiceLifetime.Transient,
                                ServiceType = type,
                            };
            #pragma warning restore IL2067

                            _registrations.Add(registration);
                            return registration;
                        }

                        public void Transfer(IServiceCollection services)
                        {
                            foreach (var registration in _registrations)
                            {
                                if (registration.Instance != null)
                                {
                                    var descriptor = ServiceDescriptor.Describe(registration.ServiceType!, f => registration.Instance, registration.Lifetime);
                                    services.Add(descriptor);
                                }
                                else
                                {
                                    var descriptor = ServiceDescriptor.Describe(registration.ServiceType!, registration.ImplementationType, registration.Lifetime);
                                    services.Add(descriptor);
                                }
                            }
                        }
                    }
            """;
}
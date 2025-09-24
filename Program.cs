using Aplicacion_de_informacion_de_una_clinica.Infraestructura;
using Aplicacion_de_informacion_de_una_clinica.Modelos;
using Aplicacion_de_informacion_de_una_clinica.puertos;
using Aplicacion_de_informacion_de_una_clinica.Servicios;
using System;

namespace Aplicacion_de_informacion_de_una_clinica
{
    class Program
    {
        static void Main(string[] args)
        {
            IRepositorioClinica repo = new InMemoryRepositorioClinica();

            var servicioUsuario = new ServicioUsuario(repo);
            var servicioPaciente = new ServicioPaciente(repo);
            var servicioOrden = new ServicioOrden(repo);
            var servicioHistoria = new ServicioHistoria(repo);
            var servicioFactura = new ServicioFactura(repo);

            // Seed: usuario RRHH y paciente e inventario de ejemplo
            try
            {
                servicioUsuario.CrearUsuario(new Usuario { NombreUsuario = "rrhh1", Contrasena = "Aa1@1234", Rol = Rol.RecursosHumanos });
            }
            catch { }

            if (repo.ObtenerPaciente("12345678") == null)
            {
                repo.AgregarPaciente(new Paciente
                {
                    Cedula = "12345678",
                    NombreCompleto = "Juan Perez",
                    FechaNacimiento = new DateTime(1990, 1, 1),
                    Telefono = "3001234567",
                    Correo = "juan@example.com",
                    Seguro = new Seguro { Compania = "Seguros S.A.", NumeroPoliza = "POL123", Activo = true, FechaVencimiento = DateTime.Now.AddMonths(6) }
                });
            }

            if (repo.ObtenerMedicamento("MED001") == null)
            {
                repo.AgregarMedicamento(new Medicamento { Id = "MED001", Nombre = "Paracetamol", Costo = 2000m, Presentacion = "Tableta" });
            }

            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== Aplicación de información de una clínica ===");
                Console.WriteLine("1. RRHH - Crear usuario");
                Console.WriteLine("2. Administrativo - Registrar paciente");
                Console.WriteLine("3. Administrativo - Crear orden (ejemplo)");
                Console.WriteLine("4. Médico - Registrar historia clínica");
                Console.WriteLine("5. Médico - Generar factura por orden");
                Console.WriteLine("0. Salir");
                Console.Write("Opción: ");
                var opt = Console.ReadLine();
                try
                {
                    switch (opt)
                    {
                        case "1":
                            CrearUsuarioMenu(servicioUsuario);
                            break;
                        case "2":
                            RegistrarPacienteMenu(servicioPaciente);
                            break;
                        case "3":
                            CrearOrdenEjemploMenu(servicioOrden, repo);
                            break;
                        case "4":
                            RegistrarHistoriaMenu(servicioHistoria);
                            break;
                        case "5":
                            GenerarFacturaMenu(servicioFactura);
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción inválida.");
                            Pausa();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pausa();
                }
            }
        }

        static void CrearUsuarioMenu(ServicioUsuario servicioUsuario)
        {
            Console.Clear();
            Console.WriteLine("=== Crear Usuario (RRHH) ===");
            Console.Write("Nombre de usuario: "); var nu = Console.ReadLine() ?? "";
            Console.Write("Contraseña: "); var pw = Console.ReadLine() ?? "";
            Console.Write("Rol (RecursosHumanos,Administrativo,Soporte,Enfermera,Medico): ");
            var rS = Console.ReadLine() ?? "RecursosHumanos";
            Enum.TryParse<Rol>(rS, out var rol);
            try
            {
                servicioUsuario.CrearUsuario(new Usuario { NombreUsuario = nu, Contrasena = pw, Rol = rol });
                Console.WriteLine("Usuario creado correctamente.");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            Pausa();
        }

        static void RegistrarPacienteMenu(ServicioPaciente servicioPaciente)
        {
            Console.Clear();
            Console.WriteLine("=== Registrar Paciente ===");
            var p = new Paciente();
            Console.Write("Cédula: "); p.Cedula = Console.ReadLine() ?? "";
            Console.Write("Nombre completo: "); p.NombreCompleto = Console.ReadLine() ?? "";
            Console.Write("Teléfono (10 dígitos): "); p.Telefono = Console.ReadLine() ?? "";
            Console.Write("Fecha de nacimiento (yyyy-mm-dd): ");
            if (DateTime.TryParse(Console.ReadLine(), out var f)) p.FechaNacimiento = f;
            try
            {
                servicioPaciente.RegistrarPaciente(p);
                Console.WriteLine("Paciente registrado.");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            Pausa();
        }

        static void CrearOrdenEjemploMenu(ServicioOrden servicioOrden, IRepositorioClinica repo)
        {
            Console.Clear();
            Console.WriteLine("=== Crear Orden (ejemplo) ===");
            Console.Write("Número de orden: "); int.TryParse(Console.ReadLine(), out var no);
            Console.Write("Cédula paciente: "); var cp = Console.ReadLine() ?? "";
            Console.Write("Cédula médico: "); var cm = Console.ReadLine() ?? "";

            // Asegurar medicamento de ejemplo
            if (repo.ObtenerMedicamento("MED001") == null)
            {
                repo.AgregarMedicamento(new Medicamento { Id = "MED001", Nombre = "Paracetamol", Costo = 2000m, Presentacion = "Tableta" });
            }

            var orden = new Orden
            {
                NumeroOrden = no,
                CedulaPaciente = cp,
                CedulaMedico = cm,
                FechaCreacion = DateTime.Now,
                Items = new System.Collections.Generic.List<ItemOrden>
                {
                    new ItemOrden { NumeroItem = 1, Tipo = TipoItem.Medicamento, IdInventario = "MED001", Cantidad = 2 }
                }
            };

            try
            {
                servicioOrden.CrearOrden(orden);
                Console.WriteLine("Orden creada correctamente.");
            }
            catch (Exception ex) { Console.WriteLine($"Error creando orden: {ex.Message}"); }
            Pausa();
        }

        static void RegistrarHistoriaMenu(ServicioHistoria servicioHistoria)
        {
            Console.Clear();
            Console.WriteLine("=== Registrar Historia Clínica ===");
            var h = new HistoriaClinica();
            Console.Write("Cédula paciente: "); h.CedulaPaciente = Console.ReadLine() ?? "";
            Console.Write("Cédula médico: "); h.CedulaMedico = Console.ReadLine() ?? "";
            Console.Write("Motivo consulta: "); h.MotivoConsulta = Console.ReadLine() ?? "";
            Console.Write("Diagnóstico (opcional): "); h.Diagnostico = Console.ReadLine() ?? "";
            Console.Write("Temperatura (opcional): "); if (decimal.TryParse(Console.ReadLine(), out var t)) h.Temperatura = t;
            try
            {
                servicioHistoria.AgregarHistoria(h);
                Console.WriteLine("Historia registrada.");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            Pausa();
        }

        static void GenerarFacturaMenu(ServicioFactura servicioFactura)
        {
            Console.Clear();
            Console.WriteLine("=== Generar Factura por Orden ===");
            Console.Write("Número de orden: "); int.TryParse(Console.ReadLine(), out var no);
            try
            {
                var factura = servicioFactura.GenerarFacturaPorOrden(no);
                Console.WriteLine($"Factura generada #{factura.NumeroFactura} - Total: {factura.Total:C2}");
                foreach (var it in factura.Items) Console.WriteLine($"{it.Descripcion} => {it.Monto:C2}");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            Pausa();
        }

        static void Pausa()
        {
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}
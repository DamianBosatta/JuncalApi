using System;
using System.Collections.Generic;

namespace JuncalApi.Modelos;

public partial class JuncalDireccionProveedor
{
    public int Id { get; set; }

    public string Direccion { get; set; }

    public bool Isdelete { get; set; }

    public int IdProveedor { get; set; }

    public virtual JuncalProveedor IdProveedorNavigation { get; set; } = null!;
}
